using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace NGU.Lab13.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class DoorTypeReporterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            Document doc = commandData.Application
                .ActiveUIDocument.Document;

            // Get all door types
            var doorTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsElementType()
                .ToElements();

            // Get all door instances to count per type
            var doorInstances = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .ToElements();

            // Count instances per type
            var instanceCounts = new Dictionary<ElementId, int>();
            foreach (Element door in doorInstances)
            {
                ElementId typeId = door.GetTypeId();
                if (instanceCounts.ContainsKey(typeId))
                    instanceCounts[typeId]++;
                else
                    instanceCounts[typeId] = 1;
            }

            // Build report
            string report = "";

            foreach (Element doorType in doorTypes)
            {
                string typeName = doorType.Name;

                double width = UnitUtils.ConvertFromInternalUnits(
                    doorType.LookupParameter("Width")?.AsDouble() ?? 0,
                    UnitTypeId.Millimeters);

                double height = UnitUtils.ConvertFromInternalUnits(
                    doorType.LookupParameter("Height")?.AsDouble() ?? 0,
                    UnitTypeId.Millimeters);

                int count = 0;
                instanceCounts.TryGetValue(doorType.Id, out count);

                report += $"Type: {typeName}\n"
                    + $"  Width: {width:F0} mm\n"
                    + $"  Height: {height:F0} mm\n"
                    + $"  Instances: {count}\n\n";
            }

            if (string.IsNullOrEmpty(report))
                report = "No door types found in the model.";

            TaskDialog.Show("Door Type Report", report);
            return Result.Succeeded;
        }
    }
}
