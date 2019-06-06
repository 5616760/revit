using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Windows.Media.Imaging;

namespace Ribbon1 {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class RibbonTest : IExternalApplication {
        public Result OnShutdown(UIControlledApplication application) {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application) {
            application.CreateRibbonTab("UCD场地工具");
            RibbonPanel panel1 = application.CreateRibbonPanel("UCD场地工具", "欢迎页");

            PushButtonData pbd = new PushButtonData("UCD场地工具", "欢迎使用", @"D:\Studay\CSharp\Work\Revit\HelloRevit\bin\Debug\HelloRevit.dll", "HelloRevit.Class1");
            PushButton pb = panel1.AddItem(pbd) as PushButton;

            RibbonPanel panel2 = application.CreateRibbonPanel("UCD场地工具", "工具");
            SplitButtonData splitData = new SplitButtonData("我的集合", "创建工具");
            SplitButton sb = panel2.AddItem(splitData) as SplitButton;

            PushButtonData spd = new PushButtonData("UCD场地工具", "创建", @"D:\Studay\CSharp\Work\Revit\Create2\bin\Debug\Create2.dll", "Create2.CreateBox") {
                LargeImage = new BitmapImage(new Uri(@"D:\Studay\CSharp\Work\Revit\Ribbon1\img\sign_road.png"))
            };
            sb.AddPushButton(spd);
            panel2.AddSeparator();

            PulldownButtonData pdbd = new PulldownButtonData("UCD场地工具", "检查");
            PushButtonData pushbtn = new PushButtonData("UCD场地工具", "碰撞检查", @"D:\Studay\CSharp\Work\Revit\Collision\bin\Debug\Collision.dll", "Collision.Class1");
            PulldownButton btn = panel2.AddItem(pushbtn) as PulldownButton;
            btn.LongDescription = "检查当前物体是否碰撞";
            btn.AddPushButton(pushbtn);

            RibbonPanel panel3 = application.CreateRibbonPanel("UCD场地工具", "文件");
            ComboBoxData cbd = new ComboBoxData("选项");
            ComboBox cBox = panel3.AddItem(cbd) as ComboBox;
            if (cBox != null) {
                cBox.ItemText = "选择操作";
                cBox.ToolTip = "请选择想要进行的操作";
                cBox.LongDescription = "选择一直接关闭，选择二关闭并修改";
                ComboBoxMemberData cbmd = new ComboBoxMemberData("A", "关闭");
                ComboBoxMemberData cbmd2 = new ComboBoxMemberData("B", "关闭并修改");
                cbmd.GroupName = "编辑操作";
                cBox.AddItem(cbmd);
                cBox.AddItem(cbmd2);
            }

            cBox.CurrentChanged += Change;
            cBox.CurrentChanged += Closed;
            return Result.Succeeded;
        }

        private void Closed(object sender, ComboBoxCurrentChangedEventArgs e) {
            TaskDialog.Show("关闭", "已关闭");
        }

        private void Change(object sender, ComboBoxCurrentChangedEventArgs e) {
            TaskDialog.Show("修改", "已修改");
        }
    }
}
