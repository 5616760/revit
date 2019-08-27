using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tunnel {
    public static class Tools {

        public static List<XYZ> ptlist = new List<XYZ>();
        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static double DegreeToR(this double d) {
            return d * Math.PI / 180;
        }
        /// <summary>
        /// 字符串转double，如果转换失败返回0，
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double SToD(string s) {
            double.TryParse(s, out double d);
            return d;
        }
        /// <summary>
        /// 坐标系转换
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="ptXyzs1"></param>
        /// <returns></returns>
        public static XYZ[] TranPts(Transform transform, XYZ[] ptXyzs1) {
            XYZ[] PT = new XYZ[ptXyzs1.Length];

            for (int i = 0; i < ptXyzs1.Length; i++) {
                PT[i] = transform.OfPoint(ptXyzs1[i]);
            }
            return PT;
        }
        /// <summary>
        /// 获取圆上的点
        /// </summary>
        /// <param name="r">半径</param>
        /// <param name="bzkAngle">标准块角度（弧度）</param>
        /// <param name="ljkAngle">连接块角度（弧度）</param>
        /// <param name="bdAngle">错缝角度</param>
        /// <returns></returns>
        public static XYZ[] GetDivPoint(double r, double bzkAngle, double ljkAngle, double bdAngle) {

            double fdkAngle = (Math.PI * 2 - bzkAngle * 3 - ljkAngle * 2) / 2;//封底块角度的一半
            double a1 = 90 + bdAngle;
            double a11 = a1 + fdkAngle;
            double a2 = a11 + ljkAngle / 2;
            double a21 = a11 + ljkAngle;
            double a3 = a21 + bzkAngle / 2;
            double a31 = a21 + bzkAngle;
            double a4 = a31 + bzkAngle / 2;
            double a41 = a31 + bzkAngle;
            double a5 = a41 + bzkAngle / 2;
            double a51 = a41 + bzkAngle;
            double a6 = a51 + ljkAngle / 2;
            double a61 = a51 + ljkAngle;

            List<double> list = new List<double> {
                a11,a2,a21,a3,a31,a4,a41,a5,a51,a6,a61,a1,a11
            };
            XYZ[] xyzs = new XYZ[13];
            for (int i = 0; i < list.Count; i++) {
                xyzs[i] = new XYZ(r * Math.Cos(list[i]), r * Math.Sin(list[i]), 0);
            }
            return xyzs;
        }
        /// <summary>
        /// 读取中心线坐标
        /// </summary>
        /// <param name="quWindow"></param>
        public static void Opencsv(TWindow quWindow) {
            OpenFileDialog ofd = new OpenFileDialog {
                Title = "请选择坐标文件",
                Filter = "(*.csv)|*.csv|(*.txt)|*.txt",
            };
            if (ofd.ShowDialog() == true) {
                string filePath = ofd.FileName;
                try {
                    StreamReader stream = new StreamReader(filePath);
                    while (stream.Peek() != -1) {
                        string line = stream.ReadLine();
                        string[] ss = line.Split(',');
                        XYZ pt = new XYZ(SToD(ss[2]) * 1000 / 304.8, SToD(ss[3]) * 1000 / 304.8, SToD(ss[4]) * 1000 / 304.8);
                        ptlist.Add(pt);
                    }
                    quWindow.LabelNum.Content = ptlist.Count.ToString();
                }
                catch (Exception exception) {
                    TaskDialog.Show("Error", exception.Message);
                }
            }
        }
        /// <summary>
        /// 生成管片
        /// </summary>
        /// <param name="quWindow"></param>
        public static void CreateTunnel(TWindow quWindow) {
            double r = SToD(quWindow.TextBoxR.Text) * 1000 / 304.8;
            double h = SToD(quWindow.TextBoxH.Text) * 1000 / 304.8;
            double w = SToD(quWindow.TextBoxW.Text) * 1000 / 304.8;
            int n = (int)SToD(quWindow.TextBoxLSGS.Text);
            string cuoFeng = quWindow.ComboBoxCuoFeng.SelectedItem.ToString();
            double BZK1 = SToD(quWindow.BZK1.Text).DegreeToR();
            double LJKN1 = SToD(quWindow.LJKN1.Text).DegreeToR();
            double LJKW1 = SToD(quWindow.LJKW1.Text).DegreeToR();
            double BZK2 = SToD(quWindow.BZK2.Text).DegreeToR();
            double LJKN2 = SToD(quWindow.LJKN2.Text).DegreeToR();
            double LJKW2 = SToD(quWindow.LJKW2.Text).DegreeToR();
            string GuanPian = quWindow.GuanPian.SelectedItem.ToString();

            if (r * h * w * n * BZK1 * BZK2 * LJKN1 * LJKN2 * LJKW1 * LJKW2 == 0) {
                TaskDialog.Show("Error", "参数输入有误，请重新输入!");
                quWindow.Activate();
            }
            else if (ptlist == null) {
                TaskDialog.Show("Error", "未选择线路坐标表或坐标表有误，请重新选择!");
                quWindow.Activate();
            }
            else {
                XYZ[] ptXyzs1 = GetDivPoint(r, BZK1, LJKW1, 0);
                XYZ[] ptXyzs2 = GetDivPoint(r - h, BZK1, LJKN1, 0);
                XYZ[] ptXyzs3 = GetDivPoint(r, BZK2, LJKW2, 0);
                XYZ[] ptXyzs4 = GetDivPoint(r - h, BZK2, LJKN2, 0);

                for (int i = 0; i < 2 - 1; i++) {
                    Line line = Line.CreateBound(ptlist[i], ptlist[i + 1]);
                    Transform transform1 = line.ComputeDerivatives(0, true);
                    //Transform transform2 = line.ComputeDerivatives(1, true);

                    XYZ[] ptXyzs11 = TranPts(transform1, ptXyzs1);
                    XYZ[] ptXyzs22 = TranPts(transform1, ptXyzs2);
                    Transform transform2 = Transform.CreateTranslation(new XYZ(0, 0, 2000 / 304.8));
                    XYZ[] ptXyzs33 = TranPts(transform2, ptXyzs3);
                    XYZ[] ptXyzs44 = TranPts(transform2, ptXyzs4);

                    TaskDialog.Show("TT!", ptXyzs3[0].ToString() + "\n" + ptXyzs33[0].ToString());
                    XYZ[] pt1 = new XYZ[12];
                    for (int k = 0; k < 3; k++) {
                        pt1[k] = ptXyzs1[k];
                    }

                    for (int l = 0; l < 3; l++) {
                        pt1[l + 3] = ptXyzs2[l];
                    }

                    for (int m = 0; m < 3; m++) {
                        pt1[m + 6] = ptXyzs33[m];
                    }

                    for (int nn = 0; nn < 3; nn++) {
                        pt1[nn + 9] = ptXyzs44[nn];
                    }
                    SimpelTunnel.CreateAdaptiveComponentFamily(pt1, GuanPian);
                }

                quWindow.Close();
                TaskDialog.Show("OK", "模型生成完毕，耗时xxx秒!");
            }
        }


        public static List<double> Cuofeng(string cuofeng, double n, double m) {
            List<double> list = new List<double>();
            double ls = 360 / n;
            switch (cuofeng) {
                case "1":
                    for (int i = 0; i < m; i++) {
                        double b = Math.Pow(-1, i) * ls;
                        list.Add(b);
                    }
                    break;
                case "2":
                    for (int i = 0; i < m; i++) {
                        double b = -Math.Pow(-1, i) * ls;
                        list.Add(b);
                    }
                    break;
                case "3":
                    for (int i = 0; i < m; i++) {
                        double b = ls * Math.Sin(i * Math.PI / 2);
                        list.Add(b);
                    }
                    break;
                case "4":
                    for (int i = 0; i < m; i++) {
                        double b = 0;
                        list.Add(b);
                        b += ls;
                    }
                    break;
                default:
                    for (int i = 0; i < m; i++) {
                        list.Add(0);
                    }
                    break;
            }
            return list;
        }
    }
}
