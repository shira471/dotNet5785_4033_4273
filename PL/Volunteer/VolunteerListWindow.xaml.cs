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
        vm = new();
        DataContext = vm; // Bind the ViewModel to the DataContext
        InitializeComponent();
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
        var calls = s_bl.Call.GetOpenCallsByVolunteer(vm.SelectedVolunteer.Id, null, null);
        int CallsNumber = calls.Count();
        return CallsNumber;
    }

    // Observer function to refresh the volunteer list when changes occur
    private void VolunteerListObserver()
    {
       vm.LoadVolunteerList();
    }

    // Event triggered when the window is loaded
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
           vm.LoadVolunteerList(); // Load the list
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

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (vm.SelectedVolunteer == null)
        {
            MessageBox.Show("No volunteer selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var selectedVolunteer = vm.SelectedVolunteer;

        // פתח את חלון הבחירה עם השם המתאים
        var actionWindow = new ActionSelectionWindow("volunteer");
        var result = actionWindow.ShowDialog();

        if (result == true)
        {
            if (actionWindow.IsUpdate) // Update
            {
                try
                {
                    var updateWindow = new AddVolunteerWindow(selectedVolunteer.Id); // פתח חלון לעדכון
                    updateWindow.ShowDialog();
                    vm.LoadVolunteerList(); // רענון הרשימה לאחר העדכון
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (actionWindow.IsDelete) // Delete
            {
                try
                {
                    s_bl.Volunteer.DeleteVolunteer(selectedVolunteer.Id); // מחיקה מה-BL
                    vm.Volunteers.Remove(vm.Volunteers.First(v => v.Id == selectedVolunteer.Id)); // הסרה מהרשימה המקומית
                    MessageBox.Show("Volunteer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        else if (actionWindow.IsCancel) // Cancel
        {
            MessageBox.Show("Action canceled.", "Cancel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
