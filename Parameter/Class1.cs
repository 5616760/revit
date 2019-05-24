using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Parameter {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document revitDoc = uiDoc.Document;
            List<ElementId> elemList = uiDoc.Selection.GetElementIds().ToList();
            Element selEle;
            ElementType type;
            foreach (ElementId id in elemList) {
                selEle = revitDoc.GetElement(id);
                type = revitDoc.GetElement(selEle.GetTypeId()) as ElementType;
                string str = "元素族名称：" + type.FamilyName + "\n元素类型：" + type.Name;
                TaskDialog.Show("元素参数", str);
            }
            return Result.Succeeded;
        }
    }
}
