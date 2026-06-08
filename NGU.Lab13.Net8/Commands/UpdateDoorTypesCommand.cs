using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace NGU.Lab13.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class UpdateDoorTypesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            Document doc = commandData.Application
                .ActiveUIDocument.Document;

            // Get all door TYPES (not instances)
            var doorTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsElementType()
                .ToElements();

            using (Transaction tx = new Transaction(doc,
                "Update Door Types"))
            {
                tx.Start();

                foreach (Element doorType in doorTypes)
                {
                    // Set type comments
                    Parameter typeComments = doorType.get_Parameter(
                        BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);

                    if (typeComments != null && !typeComments.IsReadOnly)
                    {
                        typeComments.Set("Compliant with BS 476");
                    }
                }

                tx.Commit();
            }

            TaskDialog.Show("Done",
                $"Updated {doorTypes.Count} door types.");
            return Result.Succeeded;
        }
    }
}
