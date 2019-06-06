using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using OperationCanceledException = Autodesk.Revit.Exceptions.OperationCanceledException;

namespace SelectTest {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SelectTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Selection selection = uiDoc.Selection;
            XYZ point = null;
            try {
                point = selection.PickPoint("请选择一个点");
            }
            catch (OperationCanceledException e) {
                TaskDialog.Show("Error", e.Message);
                return Result.Succeeded;
            }

            using (Transaction t1 = new Transaction(doc, "T1")) {
                t1.Start();
                FamilySymbol familySymbol = doc.GetElement(new ElementId(338370)) as FamilySymbol;
                if (familySymbol != null) {
                    if (!familySymbol.IsActive) {
                        familySymbol.Activate();
                    }
                }
                else {
                    return Result.Failed;
                }

                Level level = doc.GetElement(new ElementId(311)) as Level;
                FamilyInstance familyInstance =
                    doc.Create.NewFamilyInstance(point, familySymbol, level, StructuralType.NonStructural);
                t1.Commit();
            }

            Selection selection2 = uiDoc.Selection;
            Reference re = selection2.PickObject(ObjectType.Element, "请选择一个物体");
            Element ele = doc.GetElement(re);
            Options opt = new Options();
            GeometryElement geometry = ele.get_Geometry(opt);
            double v = 0.0;
            v = GetSolid(geometry).Sum(m => m.Volume * 0.3048 * 0.3048 * 0.3048);
            TaskDialog.Show("Hint", "选中的物体的体积为" + v.ToString("f3"));

            IList<Element> pickElements = selection2.PickElementsByRectangle(new WallSelectionFilter(), "请框选目标物体");
            double num = pickElements.Count;
            TaskDialog.Show("T", "已选中墙数为" + num);
            return Result.Succeeded;
        }

        private List<Solid> GetSolid(GeometryElement gelem) {
            List<Solid> solids = new List<Solid>();
            foreach (GeometryObject o in gelem) {
                if (o is Solid) {
                    solids.Add(o as Solid);
                }

                if (o is GeometryElement) {
                    solids.AddRange(GetSolid(o as GeometryElement));
                }

                if (o is GeometryInstance) {
                    GeometryInstance geometryInstance = o as GeometryInstance;
                    GeometryElement geometry = geometryInstance.GetInstanceGeometry();
                    solids.AddRange(GetSolid(geometry));
                }
            }
            return solids;
        }
    }
    public class WallSelectionFilter : ISelectionFilter {
        public bool AllowElement(Element elem) {
            return elem is Wall;
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return true;
        }
    }
}