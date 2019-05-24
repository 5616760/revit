using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace FiltersPractise {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            //FilterElementCollect的使用
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            //QuickFilter过滤所有窗
            collector = collector.OfCategory(BuiltInCategory.OST_Windows).OfClass(typeof(FamilySymbol));
            //FamilyInstanceFilter找1200*1500mm的窗
            IEnumerable<Element> query = from element in collector
                                         where element.Name == "1200*1500mm"
                                         select element;
            List<Element> famSyms = query.ToList<Element>();
            ElementId symbolId = famSyms[1].Id;
            FamilyInstanceFilter fiFilter = new FamilyInstanceFilter(doc, symbolId);
            FilteredElementCollector c1 = new FilteredElementCollector(doc);
            ICollection<Element> found = c1.WherePasses(fiFilter).ToElements();
            //ElementParameterFilter找到标记小于五的窗
            ElementId ruleValId = new ElementId(-10010203);
            FilterRule fr = ParameterFilterRuleFactory.CreateLessRule(ruleValId, "5", true);
            ElementParameterFilter pFilter = new ElementParameterFilter(fr);
            FilteredElementCollector c2 = new FilteredElementCollector(doc);
            c2 = c2.OfCategory(BuiltInCategory.OST_Windows).WherePasses(fiFilter).WherePasses(pFilter);
            //LogicalOrFilter计算门窗总和
            ElementCategoryFilter doorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            ElementCategoryFilter windowFilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
            LogicalOrFilter lFilter = new LogicalOrFilter(doorFilter, windowFilter);
            FilteredElementCollector c3 = new FilteredElementCollector(doc);
            ICollection<Element> fds = c3.OfClass(typeof(FamilyInstance)).WherePasses(lFilter).ToElements();
            //taskdialog输出结果
            TaskDialog.Show("查找", "已找到型号为“1200*1500mm”的推拉窗" + found.Count.ToString() +
                                  "个\n其中标记小于5的有" + c2.ToList().Count.ToString() + "个\n门窗总和为：" +
                                  fds.Count.ToString());

            return Result.Succeeded;
        }
    }
}
