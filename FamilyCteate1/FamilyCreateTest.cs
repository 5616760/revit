using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace FamilyCteate1 {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class FamilyCreateTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            UIApplication uiApp = commandData.Application;
            Application app = uiApp.Application;
            string rfePath = @"C:\ProgramData\Autodesk\RVT 2020\Family Templates\Chinese\公制柱.rft";
            Document faDoc = app.NewFamilyDocument(rfePath);
            Transaction trans = new Transaction(faDoc, "创建族");
            trans.Start();
            FamilyManager manager = faDoc.FamilyManager;
            FamilyParameter mfp =
                manager.AddParameter("材质", BuiltInParameterGroup.PG_MATERIALS, ParameterType.Material, false);
            CurveArrArray arrArray = GetCurves();
            SketchPlane sketchPlane = GetSketchPlane(faDoc);
            Extrusion extrusion = faDoc.FamilyCreate.NewExtrusion(true, arrArray, sketchPlane, 4000 / 304.8);
            faDoc.Regenerate();
            Reference topFaceReference = null;
            Options opt = new Options {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Fine
            };
            GeometryElement geometry = extrusion.get_Geometry(opt);
            foreach (GeometryObject o in geometry) {
                if (o is Solid) {
                    Solid s = o as Solid;
                    foreach (Face face in s.Faces) {
                        if (face.ComputeNormal(new UV()).IsAlmostEqualTo(new XYZ(0, 0, 1))) {
                            topFaceReference = face.Reference;
                        }
                    }
                }
            }

            View v = GetView(faDoc);
            Reference r = GetTopLevel(faDoc);
            Dimension d = faDoc.FamilyCreate.NewAlignment(v, r, topFaceReference);
            d.IsLocked = true;
            faDoc.Regenerate();
            Parameter p = extrusion.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
            manager.AssociateElementParameterToFamilyParameter(p, mfp);
            trans.Commit();
            Family fa = faDoc.LoadFamily(doc);
            faDoc.Close(false);
            trans = new Transaction(doc, "创建柱");
            trans.Start();
            fa.Name = "我的柱子";
            trans.Commit();
            return Result.Succeeded;
        }

        private Reference GetTopLevel(Document faDoc) {
            FilteredElementCollector temc = new FilteredElementCollector(faDoc);
            temc.OfClass(typeof(Level));
            Level lvl = temc.First(m => m.Name == "高于参照标高") as Level;
            return new Reference(lvl);
        }

        private View GetView(Document faDoc) {
            FilteredElementCollector viewCollector = new FilteredElementCollector(faDoc);
            viewCollector.OfClass(typeof(View));
            View v = viewCollector.First(m => m.Name == "前") as View;
            return v;
        }

        private SketchPlane GetSketchPlane(Document faDoc) {
            FilteredElementCollector teme = new FilteredElementCollector(faDoc);
            teme.OfClass(typeof(SketchPlane));
            SketchPlane sketchPlane = teme.First(m => m.Name == "低于参照标高") as SketchPlane;
            return sketchPlane;
        }

        private CurveArrArray GetCurves() {
            double len = 300 / 304.8;
            XYZ p1 = new XYZ(-len, -len, 0);
            XYZ p2 = new XYZ(len, -len, 0);
            XYZ p3 = new XYZ(len, len, 0);
            XYZ p4 = new XYZ(-len, len, 0);
            Line l1 = Line.CreateBound(p1, p2);
            Line l2 = Line.CreateBound(p2, p3);
            Line l3 = Line.CreateBound(p3, p4);
            Line l4 = Line.CreateBound(p4, p1);
            CurveArrArray arr = new CurveArrArray();
            CurveArray ar = new CurveArray();
            ar.Append(l1);
            ar.Append(l2);
            ar.Append(l3);
            ar.Append(l4);
            arr.Append(ar);
            return arr;
        }
    }
}