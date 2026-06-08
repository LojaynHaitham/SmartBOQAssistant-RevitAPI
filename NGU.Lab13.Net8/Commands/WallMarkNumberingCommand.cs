using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace NGU.Lab13.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class WallMarkNumberingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            Document doc = commandData.Application
                .ActiveUIDocument.Document;

            // Get all walls grouped by level
            var walls = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .Cast<Wall>()
                .GroupBy(w => w.get_Parameter(
                    BuiltInParameter.WALL_BASE_CONSTRAINT)
                    .AsElementId())
                .ToList();

            int totalUpdated = 0;
            string summary = "";

            using (Transaction tx = new Transaction(doc, "Number Walls"))
            {
                tx.Start();

                foreach (var levelGroup in walls)
                {
                    Level level = doc.GetElement(levelGroup.Key) as Level;
                    string levelName = level?.Name ?? "Unknown";
                    string prefix = levelName.Replace(" ", "");

                    int counter = 1;
                    foreach (Wall wall in levelGroup)
                    {
                        Parameter mark = wall.get_Parameter(
                            BuiltInParameter.ALL_MODEL_MARK);

                        if (mark != null && !mark.IsReadOnly)
                        {
                            mark.Set($"{prefix}-W{counter:D3}");
                            counter++;
                            totalUpdated++;
                        }
                    }

                    summary += $"{levelName}: {counter - 1} walls\n";
                }

                tx.Commit();
            }

            TaskDialog.Show("Wall Numbering",
                $"Updated {totalUpdated} walls:\n\n{summary}");

            return Result.Succeeded;
        }
    }
}
