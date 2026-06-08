using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NGU.Lab13.Views;

namespace NGU.Lab13.Commands
{
    /// <summary>
    /// Revit Command to show the project dashboard.
    /// Updated to instantiate the DashboardWindow with the new MVVM pattern.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    public class ShowDashboardCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            if (uiDoc == null)
            {
                message = "No active Revit document found.";
                return Result.Failed;
            }

            Document doc = uiDoc.Document;

            // Instantiate and show the WPF window using the MVVM pattern
            var window = new DashboardWindow(doc);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
