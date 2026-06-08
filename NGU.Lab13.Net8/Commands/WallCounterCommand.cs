using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGU.Lab13.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class WallCounterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // command logic
            TaskDialog.Show("Wall Counter", "The command is working ");

            return Result.Succeeded;
        }
    }
}
