using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace NGU.Lab13.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class UpdateDoorFireRatingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            Document doc = commandData.Application
                .ActiveUIDocument.Document;

            // Collect all door instances
            var doors = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();

            if (doors.Count == 0)
            {
                TaskDialog.Show("Info", "No doors found.");
                return Result.Cancelled;
            }

            int updated = 0;
            int doorNumber = 1;

            using (Transaction tx = new Transaction(doc,
                "Update Door Fire Ratings"))
            {
                tx.Start();

                foreach (FamilyInstance door in doors)
                {
                    // -- Set Fire Rating based on room --
                    string fireRating = GetFireRating(doc, door);

                    Parameter frParam = door.LookupParameter(
                        "Fire Rating");
                    if (frParam != null && !frParam.IsReadOnly)
                    {
                        frParam.Set(fireRating);
                    }

                    // -- Set sequential Mark --
                    Parameter mark = door.get_Parameter(
                        BuiltInParameter.ALL_MODEL_MARK);
                    if (mark != null && !mark.IsReadOnly)
                    {
                        mark.Set($"D-{doorNumber:D3}");
                        doorNumber++;
                    }

                    updated++;
                }

                tx.Commit();
            }

            TaskDialog.Show("Complete",
                $"Updated {updated} doors with fire ratings.");

            return Result.Succeeded;
        }

        private string GetFireRating(Document doc, FamilyInstance door)
        {
            // Get the room on the "From" side of the door
            Room fromRoom = door.FromRoom;
            Room toRoom = door.ToRoom;

            // Check room names for location clues
            string fromName = fromRoom?.get_Parameter(
                BuiltInParameter.ROOM_NAME)?.AsString()?.ToLower() ?? "";
            string toName = toRoom?.get_Parameter(
                BuiltInParameter.ROOM_NAME)?.AsString()?.ToLower() ?? "";

            // Determine fire rating
            if (fromName.Contains("stair") || toName.Contains("stair"))
                return "FD120";

            if (fromName.Contains("corridor") || toName.Contains("corridor")
                || fromName.Contains("hallway") || toName.Contains("hallway"))
                return "FD60";

            return "FD30";
        }
    }
}
