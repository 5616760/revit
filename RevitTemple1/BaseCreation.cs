using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CreateElement {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SolidTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Transaction t1 = new Transaction(doc, "T1");
            t1.Start();
            Wall wall = Wall.Create(doc, Line.CreateBound(new XYZ(), new XYZ(0, 10, 0)), Level.Create(doc, 0).Id,
                false);//注意这里需要返回Id，而不是LevelId
            t1.Commit();
            TaskDialog.Show("T1", wall.Id.ToString());
            Transaction t2 = new Transaction(doc, "copy");
            t2.Start();
            ElementTransformUtils.CopyElement(doc, wall.Id, new XYZ(10, 0, 0));
            t2.Commit();
            TaskDialog.Show("T2", "Copy Successed!");
            Transaction t3 = new Transaction(doc, "Move");
            t3.Start();
            ElementTransformUtils.MoveElement(doc, wall.Id, new XYZ(10, 20, 0));
            t3.Commit();
            TaskDialog.Show("T3", "移动完成");
            Transaction t4 = new Transaction(doc, "Mirror");
            t4.Start();
            if (ElementTransformUtils.CanMirrorElement(doc, wall.Id)) {
                Plane pl = Plane.CreateByNormalAndOrigin(new XYZ(0, -1, 0), XYZ.Zero);
                ElementTransformUtils.MirrorElement(doc, wall.Id, pl);
            }
            t4.Commit();
            TaskDialog.Show("T4", "Mirror !");
            return Result.Succeeded;
        }
    }
}