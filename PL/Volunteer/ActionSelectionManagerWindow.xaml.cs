using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using PL.Calls;
using PL.viewModel;

namespace PL
{
    /// <summary>
    /// Interaction logic for ActionSelectionManagerWindow.xaml
    /// This window allows the user to select an action (Update, Delete, View, or Cancel) for a specific entity.
    /// </summary>
    public partial class ActionSelectionManagerWindow : Window
    {
        // Singleton instance of the business logic interface (BL)
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // ViewModel instance for data binding to the UI
        public ActionManagerVM Vm
        {
            get { return (ActionManagerVM)GetValue(vMProperty); }
            set { SetValue(vMProperty, value); }
        }

        // Dependency property for the ViewModel, enabling data binding and other features like animations
        public static readonly DependencyProperty vMProperty =
            DependencyProperty.Register("Vm", typeof(ActionManagerVM), typeof(ActionSelectionManagerWindow));

        // Properties to indicate the selected action
        public bool IsUpdate { get; private set; } // Indicates if the "Update" action was chosen
        public bool IsDelete { get; private set; } // Indicates if the "Delete" action was chosen
        public bool IsCancel { get; private set; } // Indicates if the "Cancel" action was chosen
        public bool IsView { get; private set; }   // Indicates if the "View" action was chosen

        // Message to display in the window about the available actions
        public string ActionMessage { get; set; }

        // Constructor: Initializes the window and sets up the action message
        public ActionSelectionManagerWindow(string entityName)
        {
            InitializeComponent();

            // Set the action message dynamically based on the entity name
            ActionMessage = $"Select an action for the {entityName}:";

            // Set the DataContext to this window instance to enable data binding
            DataContext = this;
        }

        // Event handler for the "Update" button click
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            IsUpdate = true;      // Mark the "Update" action as selected
            DialogResult = true;  // Set the dialog result to true
            Close();              // Close the window
        }

        // Event handler for the "Delete" button click
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            IsDelete = true;      // Mark the "Delete" action as selected
            DialogResult = true;  // Set the dialog result to true
            Close();              // Close the window
        }

        // Event handler for the "View" button click
        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            IsView = true;        // Mark the "View" action as selected
            DialogResult = true;  // Set the dialog result to true
            Close();              // Close the window
        }

        // Event handler for the "Cancel" button click
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsCancel = true;      // Mark the "Cancel" action as selected
            DialogResult = true;  // Set the dialog result to true
            Close();              // Close the window
        }
    }
}
