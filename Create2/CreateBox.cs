using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Create2 {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class CreateBox : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Transaction t1 = new Transaction(doc, "Box");
            t1.Start();
            Curve c1 = Line.CreateBound(new XYZ(), new XYZ(0, 10, 0));
            Curve c2 = Line.CreateBound(new XYZ(0, 10, 0), new XYZ(10, 10, 0));
            Curve c3 = Line.CreateBound(new XYZ(10, 10, 0), new XYZ(10, 0, 0));
            Curve c4 = Line.CreateBound(new XYZ(10, 0, 0), new XYZ(0, 0, 0));
            CurveArray curveArray = new CurveArray();
            curveArray.Append(c1);
            curveArray.Append(c2);
            curveArray.Append(c3);
            curveArray.Append(c4);
            CurveArrArray curveArr = new CurveArrArray();
            curveArr.Append(curveArray);
            doc.FamilyCreate.NewExtrusion(true, curveArr,
                SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), XYZ.Zero)), 10);
            doc.FamilyManager.NewType("UCD");
            t1.Commit();

            return Result.Succeeded;
        }
    }
}