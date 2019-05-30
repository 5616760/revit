using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Trans {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class TranTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            TransactionGroup tg = new TransactionGroup(doc, "TG");
            tg.Start();
            Transaction t1 = new Transaction(doc, "T1");
            t1.Start();
            Wall.Create(doc, Line.CreateBound(new XYZ(), new XYZ(0, 10, 0)), Level.Create(doc, 0).Id, false);
            t1.Commit();
            TaskDialog.Show("T1", "已经产生第一道墙");
            Transaction t2 = new Transaction(doc, "T2");
            t2.Start();
            Wall.Create(doc, Line.CreateBound(new XYZ(), new XYZ(10, 10, 0)), Level.Create(doc, 0).Id, false);
            t2.Commit();
            tg.Assimilate();
            //tg.Commit();

            Transaction tt = new Transaction(doc, "TT");
            tt.Start();
            SubTransaction st1 = new SubTransaction(doc);
            st1.Start();
            SubTransaction st2 = new SubTransaction(doc);
            st2.Start();
            st2.Commit();
            TaskDialog.Show("ST2", "ST2已提交");
            st1.Commit();
            TaskDialog.Show("ST1", "ST1已提交");
            tt.Commit();
            return Result.Succeeded;
        }
    }
}