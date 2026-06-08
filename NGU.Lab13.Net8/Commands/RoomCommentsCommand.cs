using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace NGU.Lab13.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class RoomCommentsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            Document doc = commandData.Application
                .ActiveUIDocument.Document;

            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(r => r.Area > 0) // Skip unplaced
                .ToList();

            int updated = 0;

            using (Transaction tx = new Transaction(doc,
                "Update Room Comments"))
            {
                tx.Start();

                foreach (Room room in rooms)
                {
                    double areaSqM = UnitUtils.ConvertFromInternalUnits(
                        room.Area, UnitTypeId.SquareMeters);

                    Parameter comments = room.get_Parameter(
                        BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                    if (comments != null && !comments.IsReadOnly)
                    {
                        comments.Set($"Area: {areaSqM:F2} m2");
                        updated++;
                    }
                }

                tx.Commit();
            }

            TaskDialog.Show("Done",
                $"Updated comments for {updated} rooms.");
            return Result.Succeeded;
        }
    }
}
