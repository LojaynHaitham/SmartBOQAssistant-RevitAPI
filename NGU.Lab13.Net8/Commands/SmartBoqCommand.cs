using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NGU.Lab13.Views;

namespace NGU.Lab13.Commands
{
    /// <summary>
    /// Revit External Command that launches the Smart BOQ Assistant Dashboard.
    /// This command is registered in the Revit .addin manifest and can be triggered
    /// from the Revit user interface.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class SmartBoqCommand : IExternalCommand
    {
        /// <summary>
        /// Execution entry point for the Revit Command.
        /// </summary>
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            // 1. Get the current Revit UI document and database document
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            if (uiDoc == null)
            {
                message = "No active Revit document found.";
                return Result.Failed;
            }
            
            Document doc = uiDoc.Document;

            // 2. Instantiate and show our custom WPF window.
            // We pass the Revit Document to the window's constructor so that 
            // the ViewModel can query rooms and project info.
            var window = new DashboardWindow(doc);
            
            // ShowDialog displays the window as a modal dialog, blocking interaction 
            // with Revit until the window is closed. Since the dialog runs on the main
            // Revit thread, we can safely read Revit data inside WPF command bindings.
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}