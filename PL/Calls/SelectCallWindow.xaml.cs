using BO;
using PL.viewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Calls
{
    /// <summary>
    /// Interaction logic for SelectCallWindow.xaml
    /// </summary>
    public partial class SelectCallWindow : Window, INotifyPropertyChanged
    {
        // Event to notify the UI when a property changes
        public event PropertyChangedEventHandler? PropertyChanged;

        // Singleton instance of the business logic interface
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // ViewModel instance for data binding


        public SelectCallWindowVM Vm
        {
            get { return (SelectCallWindowVM)GetValue(vMProperty); }
            set { SetValue(vMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Vm.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty vMProperty =
            DependencyProperty.Register("Vm", typeof(SelectCallWindowVM), typeof(SelectCallWindow));

        // ID of the volunteer interacting with the window
        private int VolunteerId;

        // Constructor for initializing the window with a specific volunteer ID
        public SelectCallWindow(int volunteerId)
        {
            VolunteerId = volunteerId;

            // Initialize the ViewModel and set the DataContext for data binding
            Vm = new SelectCallWindowVM();
            InitializeComponent();

            try
            {
                // Load the call list when the window is initialized
                queryCallList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load the list of open calls and populate the ViewModel's Calls collection
        private void queryCallList()
        {
            Vm.Calls.Clear();
            try
            {
                Vm.Calls = new (s_bl.Call.GetOpenCallsByVolunteer(VolunteerId, null, null));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading the list of calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Observer method to refresh the call list when changes occur
        private void CallListObserver()
        {
            queryCallList();
        }

        // Event handler for when the window is loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                queryCallList();
                s_bl.Call.AddObserver(CallListObserver); // Add observer to track changes in call list
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading the window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handler for when the window is closed
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                s_bl.Call.RemoveObserver(CallListObserver); // Remove observer to stop tracking changes
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing the window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handler for when the ComboBox selection changes (applies a filter)
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vm.ApplyFilter();
        }

        // Event handler for the "Select" button click
        private void btnSelected_Click(object sender, RoutedEventArgs e)
        {
            // Ensure a call is selected before proceeding
            if (Vm.SelectedCall == null)
            {
                MessageBox.Show("No call selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Assign the selected call to the volunteer
                s_bl.Call.AssignCallToVolunteer(VolunteerId, Vm.SelectedCall.Description);

                // Display success message
                MessageBox.Show($"Call {Vm.SelectedCall.Id} has been assigned to volunteer {VolunteerId}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh the call list after the assignment
                queryCallList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning call to volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
