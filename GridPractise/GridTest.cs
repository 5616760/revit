using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace GridPractise {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class GridTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            FilteredElementCollector coll = new FilteredElementCollector(doc);
            ElementClassFilter gridFilter = new ElementClassFilter(typeof(Grid));
            List<Element> grid = coll.WherePasses(gridFilter).ToElements().ToList();
            List<Line> gridLines = new List<Line>();
            List<XYZ> intXyzs = new List<XYZ>();
            foreach (Grid g in grid) {
                gridLines.Add(g.Curve as Line);
            }

            foreach (Line line1 in gridLines) {
                foreach (Line line2 in gridLines) {
                    XYZ normal1 = line1.Direction;
                    XYZ normal2 = line2.Direction;
                    if (normal1.IsAlmostEqualTo(normal2)) {
                        continue;//如果平行，执行下一条
                    }

                    SetComparisonResult intRst = line1.Intersect(line2, out IntersectionResultArray results);
                    if (intRst == SetComparisonResult.Overlap && results.Size == 1) {
                        XYZ tp = results.get_Item(0).XYZPoint;
                        if (intXyzs.Where(m => m.IsAlmostEqualTo(tp)).Count() == 0) {
                            intXyzs.Add(tp);//如果不重复，则添加该交点
                        }
                    }
                }
            }

            Level level = doc.GetElement(new ElementId(311)) as Level;
            FamilySymbol familySymbol = doc.GetElement(new ElementId(338370)) as FamilySymbol;
            using (Transaction tr = new Transaction(doc)) {
                tr.Start("Clomn");
                if (!familySymbol.IsActive) {
                    familySymbol.Activate();
                }

                foreach (XYZ xyz in intXyzs) {
                    FamilyInstance familyInstance =
                        doc.Create.NewFamilyInstance(xyz, familySymbol, level, StructuralType.NonStructural);
                }

                tr.Commit();
            }

            return Result.Succeeded;
        }
    }
}