using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace NGU.Lab13.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class SetAllDoorCommentsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            Document doc = commandData.Application
                .ActiveUIDocument.Document;

            var doors = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .ToElements();
            int skipped = 0;
            using (Transaction tx = new Transaction(doc,
                "Set Door Comments"))
            {
                tx.Start();

           
                foreach (Element door in doors)
                {
                    // Skip doors inside groups
                    if (door.GroupId != ElementId.InvalidElementId)
                    {
                        skipped++;
                        continue;
                    }

                    door.get_Parameter(
                        BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
                        .Set("Reviewed - May 2026");
                }

                tx.Commit();
            }

            TaskDialog.Show("Done",
                $"Updated comments for {doors.Count - skipped} doors.\n"
                + $"Skipped {skipped} doors (inside groups).");
            return Result.Succeeded;
        }
    }
}
