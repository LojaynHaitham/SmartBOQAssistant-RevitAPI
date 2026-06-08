using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace NGU.Lab13.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateSharedParamCommand : IExternalCommand
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

            // Create the file if it doesn't exist
            if (!System.IO.File.Exists(sharedParamFile))
            {
                System.IO.File.Create(sharedParamFile).Close();
            }

            // Set it as the active shared parameter file
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
                .get_Item("Fire Rating");

            if (definition == null)
            {
                ExternalDefinitionCreationOptions options =
                    new ExternalDefinitionCreationOptions(
                        "Fire Rating", SpecTypeId.String.Text);

                definition = group.Definitions.Create(options);
            }

            // =============================================
            // Step 4: Create Category Set (which categories?)
            // =============================================
            CategorySet catSet = app.Create.NewCategorySet();

            // Add Doors category
            Category doorsCat = doc.Settings.Categories
                .get_Item(BuiltInCategory.OST_Doors);
            catSet.Insert(doorsCat);

            // Add Walls category too
            Category wallsCat = doc.Settings.Categories
                .get_Item(BuiltInCategory.OST_Walls);
            catSet.Insert(wallsCat);

            // =============================================
            // Step 5: Create Binding and Add to Document
            // =============================================
            // Instance binding = each element gets its own value
            InstanceBinding binding = app.Create
                .NewInstanceBinding(catSet);

            using (Transaction tx = new Transaction(doc,
                "Add Shared Parameter"))
            {
                tx.Start();

                BindingMap bindingMap = doc.ParameterBindings;
                bool success = bindingMap.Insert(
                    definition, binding,
                    GroupTypeId.General); // Parameter group in Properties

                tx.Commit();

                if (success)
                    TaskDialog.Show("Success",
                        "Shared parameter 'Fire Rating' added!");
                else
                    TaskDialog.Show("Info",
                        "Parameter already exists or failed.");
            }

            return Result.Succeeded;
        }
    }
}
