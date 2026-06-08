using System.Windows;
using Autodesk.Revit.DB;
using NGU.Lab13.ViewModels;

namespace NGU.Lab13.Views
{
    /// <summary>
    /// Interaction logic for DashboardWindow.xaml.
    /// Following the WPF MVVM pattern, the code-behind file should contain minimal logic.
    /// Its primary responsibility here is to hook up the ViewModel and handle UI-only events (like window closing).
    /// </summary>
    public partial class DashboardWindow : Window
    {
        /// <summary>
        /// Default parameterless constructor required for WPF designer support.
        /// </summary>
        public DashboardWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor used by Revit commands. Passes the active Revit Document
        /// to initialize our ViewModel and binds it to the DataContext.
        /// </summary>
        /// <param name="doc">The active Revit Document</param>
        public DashboardWindow(Document doc)
        {
            InitializeComponent();

            // Instantiate the DashboardViewModel and set it as the DataContext.
            // This is the magic link that connects our XAML bindings (e.g. {Binding ProjectName})
            // to the corresponding properties in the ViewModel!
            this.DataContext = new DashboardViewModel(doc);
        }

        /// <summary>
        /// Simple event handler for the Close button click.
        /// Since closing a window is a UI view operation, handling it in the code-behind is completely valid in MVVM.
        /// </summary>
        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            // Close the WPF dialog
            this.Close();
        }
    }
}
