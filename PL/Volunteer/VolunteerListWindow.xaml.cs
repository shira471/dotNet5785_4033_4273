using BO;
using PL.viewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Volunteer;

// Window for managing the list of volunteers
public partial class VolunteerListWindow : Window
{
    // Static instance for accessing the business logic layer (BL)
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // ViewModel instance for binding the data to the UI
    public VolunteerListVM vm { get; set; }

    // Constructor to initialize the window and load the volunteer list
    public VolunteerListWindow()
    {
        vm = new(); // Create a new ViewModel instance
        DataContext = vm; // Bind the ViewModel to the DataContext
        InitializeComponent(); // Initialize UI components

        try
        {
            vm.LoadVolunteerList(); // Initial loading of the volunteer list
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading volunteer list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Function to count the number of calls assigned to a volunteer
    private int VolunteerCalls(object sender, EventArgs e)
    {
        // Get a list of open calls for the selected volunteer
        var calls = s_bl.Call.GetOpenCallsByVolunteer(vm.SelectedVolunteer.Id, null, null);
        int CallsNumber = calls.Count(); // Count the calls
        return CallsNumber;
    }

    // Observer function to refresh the volunteer list when changes occur
    private void VolunteerListObserver()
    {
        vm.LoadVolunteerList(); // Reload the volunteer list
    }

    // Event triggered when the window is loaded
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            vm.LoadVolunteerList(); // Load the volunteer list
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
            s_bl?.Volunteer.RemoveObserver(VolunteerListObserver); // Remove observer to stop monitoring changes
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
            var window = new AddVolunteerWindow(); // Open the "Add Volunteer" window
            window.ShowDialog(); // Wait for the window to close
            vm.LoadVolunteerList(); // Refresh the list after adding a volunteer
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding a volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Function to navigate back (close the current window)
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        this.Close(); // Close the window
    }

    // Function to view details of the selected volunteer
    private void btnView_Click(object sender, RoutedEventArgs e)
    {
        if (vm.SelectedVolunteer == null)
        {
            // Show an error if no volunteer is selected
            MessageBox.Show("No volunteer selected for viewing.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            // Get volunteer details from the business logic layer
            var volunteerDetails = s_bl.Volunteer.GetVolunteerDetails(vm.SelectedVolunteer.Id);

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

    // Event triggered when a volunteer is double-clicked in the DataGrid
    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (vm.SelectedVolunteer == null)
        {
            // Show a message if no volunteer is selected
            MessageBox.Show("No volunteer selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var selectedVolunteer = vm.SelectedVolunteer;

        // Open the action selection window
        var actionWindow = new ActionSelectionWindow("volunteer");
        var result = actionWindow.ShowDialog();

        if (result == true)
        {
            if (actionWindow.IsUpdate) // Handle the "Update" action
            {
                try
                {
                    // Open the "Add Volunteer" window for updating
                    var updateWindow = new AddVolunteerWindow(selectedVolunteer.Id);
                    updateWindow.ShowDialog();
                    vm.LoadVolunteerList(); // Refresh the list after the update
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (actionWindow.IsDelete) // Handle the "Delete" action
            {
                try
                {
                    // Delete the volunteer using the business logic layer
                    s_bl.Volunteer.DeleteVolunteer(selectedVolunteer.Id);

                    // Remove the volunteer from the local list
                    vm.Volunteers.Remove(vm.Volunteers.First(v => v.Id == selectedVolunteer.Id));

                    MessageBox.Show("Volunteer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        else if (actionWindow.IsCancel) // Handle the "Cancel" action
        {
            MessageBox.Show("Action canceled.", "Cancel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
