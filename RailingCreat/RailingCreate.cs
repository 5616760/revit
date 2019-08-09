using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace RailingCreat {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class RailingCreate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            bool isOK= WinSelect(GetLevels(doc), GetRailings(doc), GetGuiZhens(doc), out ElementId levelId, out ElementId rId, out ElementId gId);
            if (!isOK)
            {
                return Result.Failed; 
            }
            IList<Curve> curs = GetCurves(uiDoc);//选择的曲线
            bool r1 = MyCreateRailing(rId, doc, curs, levelId);
            bool r2 = MyCreateGuizheng(gId, doc, curs, levelId);
            if (r1 && r2) {
                return Result.Succeeded;
            }
            else {
                message = "创建失败";
                return Result.Failed;
            }
        }


        private static bool WinSelect(List<Element> list, List<Element> list2, List<Element> list3, out ElementId levelId, out ElementId rId, out ElementId gId) {
            WinLevel winLevel = new WinLevel() {Width = 150,Height = 260,WindowStartupLocation = WindowStartupLocation.CenterScreen,MaxHeight = 350,MaxWidth = 250};

            list.ForEach(m => winLevel.cbLev.Items.Add(m.Name));
            winLevel.cbLev.SelectedIndex = 0;
            string level = winLevel.cbLev.SelectionBoxItem as string;
            levelId = list.First(m => m.Name == level).Id;


            list2.ForEach(m => winLevel.cbGangGui.Items.Add(m.Name));
            winLevel.cbGangGui.SelectedIndex = winLevel.cbGangGui.Items.Count-1;
            string rail = winLevel.cbGangGui.SelectionBoxItem as string;
            rId = list2.First(m => m.Name == rail).Id;


            if (list3.Count>0)//需要对轨枕族进行判断
            {
                list3.ForEach(m => winLevel.cbGuiZhen.Items.Add(m.Name));
                winLevel.cbGuiZhen.SelectedIndex = 0;
                string guizhen = winLevel.cbGuiZhen.SelectionBoxItem as string;
                gId = list3.First(m => m.Name == guizhen).Id;
            }
            else
            {
                TaskDialog.Show("Error", "没有轨枕族，请载入。");
                gId = null;
                return false;
            }

            winLevel.ShowDialog();
            return true;

        }


        private static bool MyCreateGuizheng(ElementId gId, Document doc, IList<Curve> curs, ElementId leveId) {

            if (gId != null)
            {
                DateTime oTimeStart = DateTime.Now;
                FamilySymbol guiZhenFamilySymbol = doc.GetElement(gId) as FamilySymbol;
                Level level = doc.GetElement(leveId) as Level;
                int numOfGuiZhen = 0;
                Transaction tr = new Transaction(doc); //创建对象必须添加事务
                tr.Start("轨枕");
                foreach (Curve c in curs) {
                    SubTransaction str=new SubTransaction(doc);
                    str.Start();
                    double l = c.Length * 304.8;
                    int n = (int)Math.Floor(l / 650);
                    double ang = 0;
                    if (c is Line) {
                        Line line = c as Line;
                        double lAng = line.Direction.AngleTo(XYZ.BasisX);
                        ang = line.Direction.Y >= 0 ? lAng + Math.PI / 2 : Math.PI / 2 - lAng;
                        for (double i = 0.5; i < n; i += 1) {
                            XYZ pt = c.Evaluate(i / n, true);
                            Transform t = c.ComputeDerivatives(i / n, true);
                            FamilyInstance gzInstance = doc.Create.NewFamilyInstance(pt, guiZhenFamilySymbol, level, StructuralType.NonStructural);
                            ElementTransformUtils.RotateElement(doc, gzInstance.Id, Line.CreateBound(pt, new XYZ(pt.X, pt.Y, pt.Z + 1)), ang);
                            numOfGuiZhen++;
                        }
                    }
                    else {
                        for (double i = 0.5; i < n; i += 1) {
                            Transform t = c.ComputeDerivatives(i / n, true);
                            XYZ pt = t.Origin;
                            double lAng = t.BasisY.AngleTo(new XYZ(1, 0, 0));
                            ang = t.BasisY.Y >= 0 ? lAng : -lAng;
                            FamilyInstance gzInstance = doc.Create.NewFamilyInstance(pt, guiZhenFamilySymbol, level,
                                StructuralType.NonStructural);
                            ElementTransformUtils.RotateElement(doc, gzInstance.Id,
                                Line.CreateBound(pt, new XYZ(pt.X, pt.Y, pt.Z + 1)), ang);
                            numOfGuiZhen++;
                        }
                    }

                    str.Commit();

                }
                
                tr.Commit();
                DateTime oTimeEnd=DateTime.Now;
                TimeSpan time = oTimeEnd.Subtract(oTimeStart);
                TaskDialog.Show("轨枕创建成功", $"本次共创建了{numOfGuiZhen}根轨枕！" +
                                          $"\n\t共耗时{time.Seconds}秒！" );
                return true;
            }
            else {
                TaskDialog.Show("ERR", "对象为空");
                return false;
            }
        }



        public IList<Curve> GetCurves(UIDocument uidoc) {
            IList<Curve> c = new List<Curve>();
            IList<Reference> rList = uidoc.Selection.PickObjects(ObjectType.Element);
            foreach (Reference reference in rList) {
                Element r = uidoc.Document.GetElement(reference);
                if (r is CurveElement) {
                    CurveElement m = r as CurveElement;
                    Curve curve = m.GeometryCurve;
                    c.Add(curve);
                }
                //else if(r is DetailCurve)
                //{
                //    DetailCurve m = r as DetailCurve;
                //    Curve curve = m.GeometryCurve;
                //    c.Add(curve);
                //}//只能拾取单体线段
            }
            return c;
        }

        private static bool MyCreateRailing(ElementId rId, Document doc, IList<Curve> curs, ElementId leveId) {

            if (rId != null) {
                Transaction tr = new Transaction(doc); //创建对象必须添加事务
                tr.Start("Railing");
                foreach (Curve c in curs) {
                    CurveLoop loop = new CurveLoop();
                    loop.Append(c);
                    Railing rail = Railing.Create(doc, loop, rId, leveId);
                }

                tr.Commit();
                return true;
            }
            else {
                return false;
            }
        }

        public static List<Element> GetLevels(Document doc) {

            FilteredElementCollector temc = new FilteredElementCollector(doc);
            List<Element> levels = (temc.OfClass(typeof(Level))).ToList();

            return levels;
        }
        public static List<Element> GetRailings(Document doc) {

            FilteredElementCollector temc = new FilteredElementCollector(doc);
            List<Element> railings = (temc.OfCategory(BuiltInCategory.OST_StairsRailing).OfClass(typeof(RailingType))).ToList();
            return railings;
        }

        private List<Element> GetGuiZhens(Document doc) {
            FilteredElementCollector temc = new FilteredElementCollector(doc);
            List<Element> guiZhens = (temc.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site)).ToList();
            return guiZhens;
        }
    }

}