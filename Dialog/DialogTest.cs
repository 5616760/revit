using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Dialog {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class DialogTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Application app = doc.Application;
            TaskDialog td = new TaskDialog("Title") {
                MainInstruction = "Instrution"
            };
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Line1 content", "Support content1");
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "查看产品信息");
            td.CommonButtons = TaskDialogCommonButtons.Close | TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            td.DefaultButton = TaskDialogResult.Close;
            td.ExpandedContent = "Expanded";
            td.FooterText = "<a href=\"https://www.5616760.com\">" + "关于我们</a>";
            td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
            td.TitleAutoPrefix = false;
            td.VerificationText = "不再提醒";
            TaskDialogResult result = td.Show();
            bool ischecked = td.WasVerificationChecked();
            if (result == TaskDialogResult.CommandLink1) {
                TaskDialog dialog1 = new TaskDialog("版本信息") {
                    MainInstruction = "版本名：" + app.VersionName + "\n版本号：" + app.VersionNumber
                };
                dialog1.Show();
            }
            else if (result == TaskDialogResult.CommandLink2) {
                TaskDialog.Show("信息", "sdafasdf asfdsadf sdaf g gsadf sdaf");
            }
            return Result.Succeeded;
        }
    }
}