﻿using Autodesk.Revit.Attributes;
            bool isOK= WinSelect(GetLevels(doc), GetRailings(doc), GetGuiZhens(doc), out ElementId levelId, out ElementId rId, out ElementId gId);
            bool r1 = MyCreateRailing(rId, doc, curs, levelId);


            list2.ForEach(m => winLevel.cbGangGui.Items.Add(m.Name));


        private static bool MyCreateGuizheng(ElementId gId, Document doc, IList<Curve> curs, ElementId leveId) {
                tr.Start("轨枕");
                
                tr.Commit();
            }
                tr.Start("Railing");

            return levels;