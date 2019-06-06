using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RailingCreat {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            IList<CurveLoop> curs = GetCurves(uiDoc);//选择的曲线
            ElementId rId = GetRailingId(doc);//栏杆ID
            ElementId leveId = GetLevel(doc);//标高ID
            Transaction tr = new Transaction(doc);//创建对象必须添加事务
            tr.Start("Railing");
            foreach (CurveLoop loop in curs)
            {
                Railing rail = Railing.Create(doc, loop, rId, leveId);
            }
            tr.Commit();
            return Result.Succeeded;
        }

        private ElementId GetRailingId(Document doc) {
            ElementId id = new ElementId(46430);
            //TaskDialog.Show("RailId", a);
            return id;
        }

        private ElementId GetLevel(Document doc) {
            //FilteredElementCollector temc = new FilteredElementCollector(doc);
            //temc.OfClass(typeof(Level));
            //Level lvl = temc.First(m => m.Name == "标高 1") as Level;
            ElementId id = new ElementId(13071);
            return id;
        }

        private IList<CurveLoop> GetCurves(UIDocument uidoc) {
            //Line c1 = Line.CreateBound(new XYZ(), new XYZ(0, 20000 / 304.8, 0));
            //c.Append(c1);
            IList<CurveLoop> c1=new List<CurveLoop>();
            IList<Reference> rList = uidoc.Selection.PickObjects(ObjectType.Element);
            foreach (Reference reference in rList) {
                CurveLoop c = new CurveLoop();
                var r = uidoc.Document.GetElement(reference);
                if (r is ModelCurve)
                {
                    ModelCurve m=r as ModelCurve;
                    Curve curve = m.GeometryCurve;
                    c.Append(curve);
                    c1.Add(c);
                }//只能拾取单体线段
            }
            return c1;
        }


    }
}
