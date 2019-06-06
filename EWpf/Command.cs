using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace EWpf {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class EventTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            commandData.Application.Idling += IdingTest;

            return Result.Succeeded;
        }

        private void IdingTest(object sender, IdlingEventArgs e) {
            UIApplication m_uiapp = sender as UIApplication;
            Document m_doc = m_uiapp.ActiveUIDocument.Document;
            Transaction tr = new Transaction(m_doc, "idling");
            tr.Start();
            ElementId id = new ElementId(338378);
            TextNote tn = m_doc.GetElement(id) as TextNote;
            string str = tn.Text;
            int.TryParse(str, out int i);
            tn.Text = (i + 1).ToString();
            tr.Commit();
        }
    }



}