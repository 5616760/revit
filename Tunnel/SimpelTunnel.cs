using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace Tunnel {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SimpelTunnel : IExternalCommand {
        private static UIDocument _uiDoc;
        private static Document _doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            _uiDoc = uiDoc;
            _doc = doc;

            IList<Curve> curs = GetCurves(uiDoc);//选择的曲线
            Transaction tr = new Transaction(doc, "创建隧道");
            tr.Start();
            foreach (Curve cur in curs) {
                CreateSoild(cur, 3);
            }

            tr.Commit();
            return Result.Succeeded;
        }

        private static void CreateSoild(Curve cur, double r) {
            XYZ xyz = cur.GetEndPoint(0);
            Plane p = Plane.CreateByNormalAndOrigin(cur.ComputeDerivatives(0, true).Origin, xyz);
            Arc c1 = Arc.Create(p, r * 304.8, 0, Math.PI * 2);
            XYZ xyz2 = cur.GetEndPoint(0);
            Plane p2 = Plane.CreateByNormalAndOrigin(cur.ComputeDerivatives(1, true).Origin, xyz2);
            Arc c2 = Arc.Create(p2, r * 304.8, 0, Math.PI * 2);
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
            }
            return c;
        }
        private Sweep CreateSweep(Document document, SketchPlane sketchPlane) {
            Sweep sweep = null;

            // make sure we have a family document
            if (true == document.IsFamilyDocument) {
                // Define a profile for the sweep
                CurveArrArray arrarr = new CurveArrArray();
                CurveArray arr = new CurveArray();

                // Create an ovoid profile
                XYZ pnt1 = new XYZ(0, 0, 0);
                XYZ pnt2 = new XYZ(2, 0, 0);
                XYZ pnt3 = new XYZ(1, 1, 0);
                arr.Append(Arc.Create(pnt2, 1.0d, 0.0d, 180.0d, XYZ.BasisX, XYZ.BasisY));
                arr.Append(Arc.Create(pnt1, pnt3, pnt2));
                arrarr.Append(arr);
                SweepProfile profile = document.Application.Create.NewCurveLoopsProfile(arrarr);

                // Create a path for the sweep
                XYZ pnt4 = new XYZ(10, 0, 0);
                XYZ pnt5 = new XYZ(0, 10, 0);
                Curve curve = Line.CreateBound(pnt4, pnt5);

                CurveArray curves = new CurveArray();
                curves.Append(curve);

                // create a solid ovoid sweep
                sweep = document.FamilyCreate.NewSweep(true, curves, sketchPlane, profile, 0, ProfilePlaneLocation.Start);

                if (null != sweep) {
                    // move to proper place
                    XYZ transPoint1 = new XYZ(11, 0, 0);
                    ElementTransformUtils.MoveElement(document, sweep.Id, transPoint1);
                }
                else {
                    throw new Exception("Failed to create a new Sweep.");
                }
            }
            else {
                throw new Exception("Please open a Family document before invoking this command.");
            }

            return sweep;
        }

        public static XYZ[] GetDivPoint(Arc c, double bzkAngle, double ljkAngle, double bdAngle) {
            double fdkAngle = (360 - bzkAngle * 3 - ljkAngle * 2) / 2;//封底块角度的一半
            double a1 = fdkAngle+90;
            double a2 = (fdkAngle + ljkAngle)+90;
            double a3 = (fdkAngle + ljkAngle + bzkAngle)+90;
            double a4 = (fdkAngle + ljkAngle + bzkAngle * 2)+90;
            double a5 = (fdkAngle + ljkAngle + bzkAngle * 3)+90;
            double a6 = (fdkAngle + ljkAngle * 2 + bzkAngle * 3)+90;
            List<double> list = new List<double> {
                a1,a2,a3,a4,a5,a6
            };
            XYZ[] xyzs = new XYZ[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                xyzs[i] = new XYZ(c.Radius * Math.Cos(a1), c.Radius * Math.Sin(a1), 0);
            }
            return xyzs;
        }

        public static Arc[] SplitByParameter(this Arc c, List<double> list)
        {
            double r = c.Radius;
            XYZ center =c.Center;
            Arc[] arcs = new Arc[list.Count];
            using (Transaction tr=new Transaction(_doc, "创建圆弧"))
            {
                tr.Start();
                for (int i = 0; i < list.Count; i++)
                {
                    arcs[i]=Arc.Create(center,r,list[i],list[i+1],new XYZ(1,0,0), new XYZ(0,1,0));
                }
                arcs[list.Count] = Arc.Create(center, r, list[-1], list[0], new XYZ(1, 0, 0), new XYZ(0, 1, 0));
                tr.Commit();
            }
            return arcs;
        }
    }
}