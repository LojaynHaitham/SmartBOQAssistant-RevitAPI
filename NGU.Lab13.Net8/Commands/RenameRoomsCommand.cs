using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace NGU.Lab13.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class RenameRoomsCommand : IExternalCommand
    {
        //View tab → Schedules → Schedule/Quantities
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument; //
            Document doc = uiDoc.Document; //database of the model

            // Step 1: Collect all rooms
            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms) //object style type
                .WhereElementIsNotElementType() //instance.
                .Cast<Room>() //Element
                .Where(r => r.Area > 0) // Only placed rooms
                .ToList();

            if (rooms.Count == 0)
            {
                TaskDialog.Show("Info", "No rooms found in the model.");
                return Result.Cancelled;
            }

            // Step 2: Start a Transaction
            int updatedCount = 0;
            Transaction tx = null ;
            try
            {
                using (tx= new Transaction(doc, "Rename Rooms"))
                {
                    tx.Start();

                    foreach (Room room in rooms)
                    {
                        // Get the room's level name
                        Level level = doc.GetElement(room.LevelId) as Level;
                        string levelPrefix = level?.Name ?? "Unknown";

                        // Get current room name
                        string currentName = room.get_Parameter(
                            BuiltInParameter.ROOM_NAME).AsString();

                        // Skip if already prefixed
                        if (!currentName.StartsWith(levelPrefix))
                        {
                            string newName = $"{levelPrefix}-{currentName}";
                            room.get_Parameter(
                                BuiltInParameter.ROOM_NAME).Set(newName);
                            updatedCount++;
                        }
                    }

                    tx.Commit();
                }

            }
            catch (System.Exception ex)
            {

                tx.RollBack();
            }
           
            // Step 3: Show result
            TaskDialog.Show("Done",
                $"Renamed {updatedCount} out of {rooms.Count} rooms.");

            return Result.Succeeded;
        }
    }
}
