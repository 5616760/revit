using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using Autodesk.Revit.DB.Architecture;

namespace Collision {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Transaction tran = new Transaction(doc,"ExCom");
            tran.Start();
            Selection select = uiDoc.Selection;
            Reference r = select.PickObject(ObjectType.Element, "选择需要检查的墙");
            Element column = doc.GetElement(r);
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            //使用ElementIntersectFilter冲突检查
            ElementIntersectsElementFilter iFilter = new ElementIntersectsElementFilter(column, false);
            collector.WherePasses(iFilter);
            List<ElementId> excludes = new List<ElementId> {
                column.Id
            };
            collector.Excluding(excludes);
            List<ElementId> ids = new List<ElementId>();
            select.SetElementIds(ids);
            foreach (Element element in collector) {
                ids.Add(element.Id);
            }
            select.SetElementIds(ids);
            tran.Commit();
            CurveLoop curs=null;
            ElementId rId=null;
            ElementId leveId=null;
            Railing rail=Railing.Create(doc,curs,rId,leveId);
            return Result.Succeeded;
        }
    }
}
