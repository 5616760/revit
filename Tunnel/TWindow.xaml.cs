using Autodesk.Revit.UI;
using System.Windows;
using System.Windows.Controls;

namespace Tunnel {
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class TWindow : Window {
        public TWindow() {
            InitializeComponent();
        }

        private void ButtonPick_Click(object sender, RoutedEventArgs e) {
            Tools.Opencsv(this);
            Activate();
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e) {
            Tools.CreateTunnel(this);
            Close();
        }

        /// <summary>
        /// WPFTextBox只能输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxR_TextChanged(object sender, TextChangedEventArgs e) {
            System.Windows.Controls.TextBox temptbox = sender as System.Windows.Controls.TextBox;               //此句可能是为保护原框，也可能只是为了用一下sender -.-
            TextChange[] change = new TextChange[e.Changes.Count];         //
            e.Changes.CopyTo(change, 0);                     //得到Change的内容
            int offset = change[0].Offset;                    //得到Change的偏置值(offset) 可理解为变化的起始位置
            if (change[0].AddedLength > 0)                                  //如果是内容增加 则执行
            {
                //其实没啥用 但是没这个变量TryParse函数不能用
                if (temptbox.Text.IndexOf(' ') != -1 || !double.TryParse(temptbox.Text, out double num)) {                                  //Text.IndexOf检测某字符首次出现的位置，此处用来检测是否有空格
                    //int.TryParse返回是字符串是否转为数字，此处用来检测字符串是纯数字
                    //float，double应该也有类似的函数，可以用来实现小数检测
                    temptbox.Text = temptbox.Text.Remove(offset, change[0].AddedLength);//去除change
                    TaskDialog.Show("输入错误", "只能输入数字，请重新输入！");
                    Activate();
                    temptbox.Select(offset, 0);　　　　　　　　　　　　　　　　　　　　　　　　//恢复原状
                }
            }
        }
        
    }
}
