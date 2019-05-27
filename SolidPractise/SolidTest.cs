using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace SolidPractise {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SolidTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            ICollection<ElementId> ids = uiDoc.Selection.GetElementIds();
            Element selElem = uiDoc.Document.GetElement(ids.First());
            GeometryElement ge = selElem.get_Geometry(new Options());
            double area = 0;
            double volume = 0;
            int triangleCount = 0;
            foreach (GeometryObject o in ge) {
                if (o is Solid) {
                    Solid sd = o as Solid;
                    foreach (Face face in sd.Faces) {
                        area += face.Area * 0.3048 * 0.3048;//这里计算了所有的面，所以和特性中显示的不一致。
                        Mesh mesh = face.Triangulate(0.5);
                        triangleCount += mesh.NumTriangles;
                    }

                    volume += sd.Volume * 0.3048 * 0.3048 * 0.3048;
                }
            }

            TaskDialog.Show("计算", "面积总和为：" + area.ToString() + "平方米\n" +
                                  "体积为：" + volume.ToString("0.000") + "立方米\n" +
                                  "三角网格数为：" + triangleCount.ToString());

            return Result.Succeeded;
        }
    }
}