using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace NGU.Lab13.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateInspectionDateCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            // =============================================
            // Step 1: Open or Create Shared Parameter File
            // =============================================
            string dllDir = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            string sharedParamFile = System.IO.Path.Combine(dllDir, "SharedParams.txt");

            if (!System.IO.File.Exists(sharedParamFile))
            {
                System.IO.File.Create(sharedParamFile).Close();
            }

            app.SharedParametersFilename = sharedParamFile;
            DefinitionFile defFile = app.OpenSharedParameterFile();

            // =============================================
            // Step 2: Create or Get a Group
            // =============================================
            DefinitionGroup group = defFile.Groups
                .get_Item("My Custom Parameters");

            if (group == null)
            {
                group = defFile.Groups.Create("My Custom Parameters");
            }

            // =============================================
            // Step 3: Create the Parameter Definition
            // =============================================
            Definition definition = group.Definitions
                .get_Item("Inspection Date");

            if (definition == null)
            {
                ExternalDefinitionCreationOptions options =
                    new ExternalDefinitionCreationOptions(
                        "Inspection Date", SpecTypeId.String.Text);

                definition = group.Definitions.Create(options);
            }

            // =============================================
            // Step 4: Create Category Set for Doors
            // =============================================
            CategorySet catSet = app.Create.NewCategorySet();

            Category doorsCat = doc.Settings.Categories
                .get_Item(BuiltInCategory.OST_Doors);
            catSet.Insert(doorsCat);

            // =============================================
            // Step 5: Create Instance Binding and Add
            // =============================================
            InstanceBinding binding = app.Create
                .NewInstanceBinding(catSet);

            using (Transaction tx = new Transaction(doc,
                "Add Inspection Date Parameter"))
            {
                tx.Start();

                BindingMap bindingMap = doc.ParameterBindings;
                bindingMap.Insert(definition, binding,
                    GroupTypeId.General);

                tx.Commit();
            }

            // =============================================
            // Step 6: Set today's date for all doors
            // =============================================
            var doors = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .ToElements();

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            int updated = 0;

            using (Transaction tx = new Transaction(doc,
                "Set Inspection Dates"))
            {
                tx.Start();

                foreach (Element door in doors)
                {
                    Parameter param = door.LookupParameter("Inspection Date");
                    if (param != null && !param.IsReadOnly)
                    {
                        param.Set(today);
                        updated++;
                    }
                }

                tx.Commit();
            }

            TaskDialog.Show("Done",
                $"Created 'Inspection Date' parameter and set to {today} for {updated} doors.");
            return Result.Succeeded;
        }
    }
}
