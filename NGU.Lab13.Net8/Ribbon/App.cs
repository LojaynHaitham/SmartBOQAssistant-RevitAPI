using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using NGU.Lab13.Net8.Properties;

namespace NGU.Lab13.Ribbon
{
    /// <summary>
    /// Entry point for the Revit add-in. Creates a custom ribbon tab
    /// with organized panels and push buttons for all Lab13 commands.
    /// </summary>
    public class App : IExternalApplication
    {
        // Command namespace prefix
        private const string CommandNamespace = "NGU.Lab13.Commands";

        public Result OnStartup(UIControlledApplication application)
        {
            // Step 1: Create a custom ribbon tab
            string tabName = "NGU Lab13";
            application.CreateRibbonTab(tabName);

            // Step 2: Add organized panels with push buttons
            CreateDoorPanel(application, tabName);
            CreateWallPanel(application, tabName);
            CreateRoomPanel(application, tabName);
            CreateParameterPanel(application, tabName);

            // Step 3: Demo panels showcasing different ribbon element types
            CreateSplitButtonPanel(application, tabName);
            CreatePulldownButtonPanel(application, tabName);
            CreateStackedItemsPanel(application, tabName);
            CreateTextBoxComboBoxPanel(application, tabName);
            CreateWpfPanel(application, tabName);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        #region Panel Creation Methods

        /// <summary>
        /// Creates the "Doors" panel with door-related commands.
        /// </summary>
        private void CreateDoorPanel(UIControlledApplication app, string tabName)
        {
            RibbonPanel panel = app.CreateRibbonPanel(tabName, "Doors");

            // Primary action - large button
            RibbonHelper.AddPushButton(panel,
                "btnUpdateFireRating",
                "Update\nFire Rating",
                $"{CommandNamespace}.UpdateDoorFireRatingCommand",
                "Sets fire rating for all doors based on adjacent room names (Stair=FD120, Corridor=FD60, Default=FD30)");

            // Separator for visual grouping
            panel.AddSeparator();

            // Secondary door actions
            RibbonHelper.AddPushButton(panel,
                "btnSetDoorComments",
                "Set Door\nComments",
                $"{CommandNamespace}.SetAllDoorCommentsCommand",
                "Sets the same Comments value for all door instances (skips grouped doors)");

            RibbonHelper.AddPushButton(panel,
                "btnUpdateDoorTypes",
                "Update\nDoor Types",
                $"{CommandNamespace}.UpdateDoorTypesCommand",
                "Sets type comments to 'Compliant with BS 476' for all door types");

            panel.AddSeparator();

            // Reporting / read-only
            RibbonHelper.AddPushButton(panel,
                "btnDoorTypeReport",
                "Door Type\nReport",
                $"{CommandNamespace}.DoorTypeReporterCommand",
                "Generates a report of all door types with dimensions and instance counts");

            RibbonHelper.AddPushButton(panel,
                "btnInspectionDate",
                "Inspection\nDate",
                $"{CommandNamespace}.CreateInspectionDateCommand",
                "Creates an Inspection Date shared parameter and sets today's date for all doors");
        }

        /// <summary>
        /// Creates the "Walls" panel with wall-related commands.
        /// </summary>
        private void CreateWallPanel(UIControlledApplication app, string tabName)
        {
            RibbonPanel panel = app.CreateRibbonPanel(tabName, "Walls");

            RibbonHelper.AddPushButton(panel,
                "btnWallCounter",
                "Wall\nCounter",
                $"{CommandNamespace}.WallCounterCommand",
                "Counts all walls in the current model",
                Resources.wall_icon_32,
                Resources.wall_icon_16);

            RibbonHelper.AddPushButton(panel,
                "btnWallMarkNumbering",
                "Wall Mark\nNumbering",
                $"{CommandNamespace}.WallMarkNumberingCommand",
                "Assigns sequential mark numbers to all walls, grouped by level (e.g. Ground-W001)",
                Resources.wall_icon_32,
                Resources.wall_icon_16);
        }

        /// <summary>
        /// Creates the "Rooms" panel with room-related commands.
        /// </summary>
        private void CreateRoomPanel(UIControlledApplication app, string tabName)
        {
            RibbonPanel panel = app.CreateRibbonPanel(tabName, "Rooms");

            RibbonHelper.AddPushButton(panel,
                "btnRenameRooms",
                "Rename\nRooms",
                $"{CommandNamespace}.RenameRoomsCommand",
                "Adds floor level prefix to all room names (e.g. Level1-Kitchen)");

            RibbonHelper.AddPushButton(panel,
                "btnRoomComments",
                "Room\nComments",
                $"{CommandNamespace}.RoomCommentsCommand",
                "Sets room comments to include the room area in square meters");
        }

        /// <summary>
        /// Creates the "Parameters" panel with shared parameter commands.
        /// </summary>
        private void CreateParameterPanel(UIControlledApplication app, string tabName)
        {
            RibbonPanel panel = app.CreateRibbonPanel(tabName, "Parameters");

            RibbonHelper.AddPushButton(panel,
                "btnCreateSharedParam",
                "Create\nFire Rating",
                $"{CommandNamespace}.CreateSharedParamCommand",
                "Creates a Fire Rating shared parameter and binds it to Doors and Walls categories");
        }

        /// <summary>
        /// Demo: SplitButton — a button with a default click action
        /// and a dropdown arrow revealing alternative commands.
        /// The top (large) button executes the current/default command;
        /// the dropdown lets the user pick a different one.
        /// </summary>
        private void CreateSplitButtonPanel(UIControlledApplication app, string tabName)
        {
            RibbonPanel panel = app.CreateRibbonPanel(tabName, "Split Button");

            // Create the SplitButton container
            var splitButtonData = new SplitButtonData("splitDoorActions", "Door\nActions");
            SplitButton splitButton = panel.AddItem(splitButtonData) as SplitButton;

            // Add commands — the first one becomes the default (large icon) action
            splitButton.AddPushButton(RibbonHelper.CreatePushButtonData(
                "splitFireRating",
                "Update Fire Rating",
                $"{CommandNamespace}.UpdateDoorFireRatingCommand",
                "Sets fire rating for all doors based on adjacent room names"));

            splitButton.AddPushButton(RibbonHelper.CreatePushButtonData(
                "splitDoorComments",
                "Set Door Comments",
                $"{CommandNamespace}.SetAllDoorCommentsCommand",
                "Sets the same Comments value for all door instances"));

            splitButton.AddPushButton(RibbonHelper.CreatePushButtonData(
                "splitDoorTypes",
                "Update Door Types",
                $"{CommandNamespace}.UpdateDoorTypesCommand",
                "Sets type comments for all door types"));
        }

        /// <summary>
        /// Demo: PulldownButton — a dropdown-only button (no default click).
        /// Always shows the arrow; user must pick from the list.
        /// </summary>
        private void CreatePulldownButtonPanel(UIControlledApplication app, string tabName)
        {
            RibbonPanel panel = app.CreateRibbonPanel(tabName, "Pulldown Button");

            // Create the PulldownButton container with report icon
            var pulldownData = new PulldownButtonData("pulldownReports", "Reports\nMenu");
            pulldownData.LargeImage = RibbonHelper.BitmapToBitmapSource(Resources.report_icon_32);
            pulldownData.Image = RibbonHelper.BitmapToBitmapSource(Resources.report_icon_16);
            PulldownButton pulldownButton = panel.AddItem(pulldownData) as PulldownButton;

            // Add commands to the dropdown list
            pulldownButton.AddPushButton(RibbonHelper.CreatePushButtonData(
                "pullDoorReport",
                "Door Type Report",
                $"{CommandNamespace}.DoorTypeReporterCommand",
                "Generates a report of all door types with dimensions and instance counts"));

            pulldownButton.AddPushButton(RibbonHelper.CreatePushButtonData(
                "pullWallCounter",
                "Wall Counter",
                $"{CommandNamespace}.WallCounterCommand",
                "Counts all walls in the current model"));

            pulldownButton.AddPushButton(RibbonHelper.CreatePushButtonData(
                "pullRoomComments",
                "Room Comments",
                $"{CommandNamespace}.RoomCommentsCommand",
                "Sets room comments to include the room area in square meters"));
        }

        /// <summary>
        /// Demo: Stacked Items — 2 or 3 small buttons stacked vertically
        /// in a single ribbon column. Saves horizontal space.
        /// </summary>
        private void CreateStackedItemsPanel(UIControlledApplication app, string tabName)
        {
            RibbonPanel panel = app.CreateRibbonPanel(tabName, "Stacked Items");

            // Stack of 3 small buttons (max allowed by Revit API)
            var btn1 = RibbonHelper.CreatePushButtonData(
                "stackRenameRooms",
                "Rename Rooms",
                $"{CommandNamespace}.RenameRoomsCommand",
                "Adds floor level prefix to all room names");

            var btn2 = RibbonHelper.CreatePushButtonData(
                "stackWallNumbering",
                "Wall Numbering",
                $"{CommandNamespace}.WallMarkNumberingCommand",
                "Assigns sequential mark numbers to walls by level");

            var btn3 = RibbonHelper.CreatePushButtonData(
                "stackInspectionDate",
                "Inspection Date",
                $"{CommandNamespace}.CreateInspectionDateCommand",
                "Creates Inspection Date parameter for all doors");

            panel.AddStackedItems(btn1, btn2, btn3);

            panel.AddSeparator();

            // Stack of 2 small buttons
            var btnA = RibbonHelper.CreatePushButtonData(
                "stackSharedParam",
                "Create Param",
                $"{CommandNamespace}.CreateSharedParamCommand",
                "Creates a Fire Rating shared parameter");

            var btnB = RibbonHelper.CreatePushButtonData(
                "stackDoorReport",
                "Door Report",
                $"{CommandNamespace}.DoorTypeReporterCommand",
                "Generates a door type report");

            panel.AddStackedItems(btnA, btnB);
        }

        /// <summary>
        /// Demo: TextBox and ComboBox — non-command ribbon controls.
        /// TextBox accepts free-text input; ComboBox provides a dropdown selection.
        /// Both fire events when the user interacts with them.
        /// </summary>
        private void CreateTextBoxComboBoxPanel(UIControlledApplication app, string tabName)
        {
            RibbonPanel panel = app.CreateRibbonPanel(tabName, "Input Controls");

            // --- TextBox: free-text input on the ribbon ---
            var textBoxData = new TextBoxData("txtUserInput");
            TextBox textBox = panel.AddItem(textBoxData) as TextBox;
            textBox.PromptText = "Type a value...";
            textBox.ToolTip = "Demo: enter any text and press Enter to see a TaskDialog";
            textBox.Width = 200;
            textBox.EnterPressed += OnTextBoxEnterPressed;

            panel.AddSeparator();

            // --- ComboBox: dropdown selection ---
            var comboBoxData = new ComboBoxData("cmbSelection");
            ComboBox comboBox = panel.AddItem(comboBoxData) as ComboBox;
            comboBox.ToolTip = "Demo: select an option from the dropdown";

            // Add members (options) to the ComboBox
            comboBox.AddItem(new ComboBoxMemberData("optDoors", "Doors"));
            comboBox.AddItem(new ComboBoxMemberData("optWalls", "Walls"));
            comboBox.AddItem(new ComboBoxMemberData("optRooms", "Rooms"));
            comboBox.AddItem(new ComboBoxMemberData("optAll", "All Categories"));

            comboBox.CurrentChanged += OnComboBoxChanged;
        }

        /// <summary>
        /// Demo: WPF Window — a ribbon button that opens a custom WPF dialog.
        /// Shows how to launch a full UI from a Revit command.
        /// </summary>
        private void CreateWpfPanel(UIControlledApplication app, string tabName)
        {
            RibbonPanel panel = app.CreateRibbonPanel(tabName, "WPF");

            RibbonHelper.AddPushButton(panel,
                "btnShowDashboard",
                "Project\nDashboard",
                $"{CommandNamespace}.ShowDashboardCommand",
                "Opens a WPF window showing project summary: wall/door counts and room list",
                Resources.report_icon_32,
                Resources.report_icon_16);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Fired when the user presses Enter in the TextBox.
        /// </summary>
        private void OnTextBoxEnterPressed(object sender, TextBoxEnterPressedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            string value = textBox?.Value?.ToString() ?? "(empty)";
            TaskDialog.Show("TextBox Demo", $"You entered: {value}");
        }

        /// <summary>
        /// Fired when the user changes the ComboBox selection.
        /// </summary>
        private void OnComboBoxChanged(object sender, ComboBoxCurrentChangedEventArgs args)
        {
            ComboBox comboBox = sender as ComboBox;
            string selected = comboBox?.Current?.Name ?? "(none)";
            TaskDialog.Show("ComboBox Demo", $"Selected category: {selected}");
        }

        #endregion
    }
}
