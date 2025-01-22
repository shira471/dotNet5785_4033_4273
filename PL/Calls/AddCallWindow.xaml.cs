using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using PL.Volunteer;
using BO;

namespace PL.Call
{
    /// <summary>
    /// Interaction logic for AddCallWindow.xaml
    /// This window is used for adding or updating a call. It contains a form where the user can input call details.
    /// </summary>
    public partial class AddCallWindow : Window
    {
        // ObservableCollection to bind the available call types (enum) to the UI for selection
        public ObservableCollection<CallType> CallTypes { get; set; }

        // DependencyProperty for the CurrentCall object, which represents the call being added or updated
        public BO.Call? CurrentCall
        {
            get { return (BO.Call?)GetValue(CurrentCallProperty); }
            set { SetValue(CurrentCallProperty, value); }
        }

        // DependencyProperty for the button text (Add/Update)
        public static readonly DependencyProperty CurrentCallProperty =
            DependencyProperty.Register("CurrentCall", typeof(BO.Call), typeof(AddCallWindow), new PropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        // DependencyProperty for the button text (Add/Update)
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(AddCallWindow), new PropertyMetadata("Add"));

        // A reference to the business logic layer for interacting with the backend
        private readonly BlApi.IBl s_bl;

        /// <summary>
        /// Constructor for AddCallWindow.
        /// Initializes the window in either Add or Update mode based on the provided call ID.
        /// </summary>
        public AddCallWindow(int id = 0)
        {
            InitializeComponent();

            // Initialize the CallTypes ObservableCollection with all possible values of the CallType enum
            CallTypes = new ObservableCollection<CallType>(Enum.GetValues(typeof(CallType)).Cast<CallType>());

            // Factory pattern to get the BL instance for accessing business logic
            s_bl = BlApi.Factory.Get();

            if (id == 0)
            {
                // Add mode: Initialize CurrentCall with default values for a new call
                CurrentCall = new BO.Call
                {
                    CallType = CallType.Breakfast,            // Default to "Breakfast" type
                    OpenTime = DateTime.Now                   // Default to the current date and time
                };
                ButtonText = "Add"; // Set the button text to "Add"
            }
            else
            {
                // Update mode: Load the call details from the backend using the provided ID
                try
                {
                    CurrentCall = s_bl.Call.GetCallDetails(id.ToString());
                    ButtonText = "Update"; // Set the button text to "Update"
                }
                catch (Exception ex)
                {
                    // If there is an error fetching the call details, display the error and close the window
                    MessageBox.Show($"Error loading call: {ex.Message}");
                    Close();
                }
            }

            // Set the DataContext to this class, enabling data binding in XAML
            DataContext = this;
        }

        /// <summary>
        /// Event handler for the Add/Update button click.
        /// Adds a new call or updates an existing one based on the ButtonText property.
        /// </summary>
        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ButtonText == "Add")
                {
                    // If in Add mode, call the AddCall method from BL to add the new call
                    s_bl.Call.AddCall(CurrentCall!);
                    MessageBox.Show("Call added successfully.");
                }
                else
                {
                    // If in Update mode, call the UpdateCallDetails method from BL to update the call
                    s_bl.Call.UpdateCallDetails(CurrentCall);
                    MessageBox.Show("Call updated successfully.");
                }

                // Close the window after a successful operation
                Close();
            }
            catch (Exception ex)
            {
                // Display an error message if something goes wrong
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for the Back button click.
        /// Closes the current window without making any changes.
        /// </summary>
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Close the current window and return to the previous window
        }

        /// <summary>
        /// Event handler to restrict input to numeric characters only for certain fields (e.g., ID).
        /// </summary>
        private void NumericOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Prevent non-numeric input by checking if the input text is a valid integer
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}
