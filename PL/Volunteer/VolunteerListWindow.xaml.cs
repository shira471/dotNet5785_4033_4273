using BO;
using PL.viewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Volunteer
{
    // Window for managing the list of volunteers
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Static instance for accessing the business logic layer (BL)
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // ViewModel instance for binding the data to the UI
        public VolunteerListVM vm { get; set; }

        // Constructor to initialize the window and load the volunteer list
        public VolunteerListWindow()
        {
            vm = new();
            DataContext = vm; // Bind the ViewModel to the DataContext
            InitializeComponent();
            try
            {
                LoadVolunteerList(); // Initial loading of the volunteer list
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading volunteer list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Function to load the volunteer list from the BL
        private void LoadVolunteerList()
        {
            vm.Volunteers.Clear(); // Clear the existing list
            try
            {
                var volunteers = s_bl?.Volunteer.GetVolunteersList(null, vm.VolunteerSortBy) ?? Enumerable.Empty<BO.VolunteerInList>();
                foreach (var volunteer in volunteers)
                {
                    vm.Volunteers.Add(volunteer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading volunteer list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Function to count the number of calls assigned to a volunteer
        private int VolunteerCalls(object sender, EventArgs e)
        {
            var calls = s_bl.Call.GetOpenCallsByVolunteer(vm.SelectedVolunteer.Id, null, null);
            int CallsNumber = calls.Count();
            return CallsNumber;
        }

        // Observer function to refresh the volunteer list when changes occur
        private void VolunteerListObserver()
        {
            LoadVolunteerList();
        }

        // Event triggered when the window is loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadVolunteerList(); // Load the list
                s_bl?.Volunteer.AddObserver(VolunteerListObserver); // Add observer to monitor changes
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading the window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event triggered when the window is closed
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                s_bl?.Volunteer.RemoveObserver(VolunteerListObserver); // Remove observer to stop monitoring
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing the window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Function to add a new volunteer
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new AddVolunteerWindow(); // Open the add volunteer window
                window.ShowDialog();

                LoadVolunteerList(); // Refresh the list after adding a volunteer
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding a volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Function to delete a selected volunteer
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var SelectedVolunteer = vm.SelectedVolunteer;
            if (SelectedVolunteer == null) // Check if a volunteer is selected
            {
                MessageBox.Show("No volunteer selected for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the volunteer {SelectedVolunteer.FullName}?",
                "Delete Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes) // Proceed with deletion if confirmed
            {
                try
                {
                    s_bl.Volunteer.DeleteVolunteer(SelectedVolunteer.Id); // Delete the volunteer in BL
                    vm.Volunteers.Remove(vm.Volunteers.First(v => v.Id == SelectedVolunteer.Id)); // Remove from local list
                    MessageBox.Show("Volunteer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to delete volunteer: {ex.Message}", "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Function to navigate back (close the current window)
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Function to view details of the selected volunteer
        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedVolunteer == null)
            {
                MessageBox.Show("No volunteer selected for viewing.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                var volunteerDetails = s_bl.Volunteer.GetVolunteerDetails(vm.SelectedVolunteer.Id); // Get volunteer details from BL

                // Construct a string with the volunteer's details
                string details = $"Name: {volunteerDetails.FullName}\n" +
                                 $"Phone Number: {volunteerDetails.Phone}\n" +
                                 $"Role: {volunteerDetails.Role}";

                // Display the details in a message box
                MessageBox.Show(details, "Volunteer Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to view volunteer details: {ex.Message}", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Function to update the selected volunteer
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedVolunteer == null)
            {
                MessageBox.Show("No volunteer selected for updating.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var window = new AddVolunteerWindow(vm.SelectedVolunteer.Id); // Open the update volunteer window
                window.ShowDialog();
                LoadVolunteerList(); // Refresh the list after updating
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
