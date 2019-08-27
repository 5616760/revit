using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Windows;

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
            List<Element> list = GetGuanPian(doc);
            TWindow quWindow = new TWindow() { WindowStartupLocation = WindowStartupLocation.CenterScreen, MaxHeight = 320, MaxWidth = 500 };
            quWindow.ComboBoxCuoFeng.SelectedIndex = 0;
            list.ForEach(m => quWindow.GuanPian.Items.Add(m.Name));
            quWindow.GuanPian.SelectedIndex = list.Count - 1;
            quWindow.ShowDialog();
            return Result.Succeeded;
        }

        private List<Element> GetGuanPian(Document doc) {
            FilteredElementCollector temc = new FilteredElementCollector(doc);
            List<Element> guanPian = temc.OfClass(typeof(FamilySymbol)).ToElements() as List<Element>;
            return guanPian;
        }


        public static void CreateAdaptiveComponentFamily(XYZ[] pts, string fName) {
            FilteredElementCollector temc = new FilteredElementCollector(_doc);
            List<Element> guanPian = temc.OfClass(typeof(FamilySymbol)).ToElements() as List<Element>;
            FamilySymbol symbol = null;
            foreach (Element element in guanPian) {
                if (element.Name == fName) {
                    symbol = element as FamilySymbol;
                }
            }

            //创建自适应族
            using (Transaction tr = new Transaction(_doc)) {
                tr.Start("自适应族");
                FamilyInstance instance = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(_doc, symbol);

                //获取实例的放置点
                IList<ElementId> placePointIds = new List<ElementId>();
                placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(instance);

                //设置每个放置点的位置
                for (int i = 0; i < placePointIds.Count; i++) {
                    ReferencePoint point = _doc.GetElement(placePointIds[i]) as ReferencePoint;
                    point.Position = pts[i];
                }

                tr.Commit();
            }
        }


    }
}