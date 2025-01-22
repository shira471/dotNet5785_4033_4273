using BO;
using DO;
using PL.viewModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Calls;

/// <summary>
/// Interaction logic for SelectCallWindow.xaml
/// This window allows selecting and handling calls for a volunteer.
/// </summary>
public partial class SelectCallWindow : Window
{
    // Singleton instance of the business logic interface (BL)
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // ViewModel instance for data binding, holds logic and data for the calls selection
    public SelectCallWindowVM Vm
    {
        get { return (SelectCallWindowVM)GetValue(vMProperty); }
        set { SetValue(vMProperty, value); }
    }

    // DependencyProperty to store the ViewModel for this window
    public static readonly DependencyProperty vMProperty =
        DependencyProperty.Register("Vm", typeof(SelectCallWindowVM), typeof(SelectCallWindow));

    // Constructor for initializing the window with the specified volunteer ID
    public SelectCallWindow(int volunteerId)
    {
        InitializeComponent();
        try
        {
            // Initialize the ViewModel with the volunteer ID
            Vm = new SelectCallWindowVM(volunteerId);
        }
        catch (Exception ex)
        {
            // Display an error message if loading calls fails
            MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Fetches the call list for the volunteer by loading it from the ViewModel.
    /// </summary>
    private void queryCallList()
    {
        try
        {
            Vm.LoadCalls(); // Calls ViewModel method to load calls
        }
        catch (Exception ex)
        {
            // Display an error message if loading the calls list fails
            MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Observes call list updates and triggers loading the list of calls.
    /// </summary>
    private void callListObserver()
    {
        queryCallList();
    }

    /// <summary>
    /// Event handler for the window loading event. It triggers the call list load when the window is opened.
    /// </summary>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        queryCallList(); // Load calls when the window is loaded
    }

    /// <summary>
    /// Event handler for the window closing event. Removes the observer on window close.
    /// </summary>
    private void Window_Closed(object sender, EventArgs e)
    {
        try
        {
            // Remove the observer that listens to call updates
            s_bl?.Call.RemoveObserver(callListObserver);
        }
        catch (Exception ex)
        {
            // Display an error message if an issue occurs when closing the window
            MessageBox.Show($"Error closing window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Event handler for the data grid's mouse double-click event.
    /// Assigns a selected call to a volunteer after confirmation.
    /// </summary>
    private void CallsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var selectedCall = Vm.Selected; // Get the selected call from the ViewModel
        if (selectedCall == null)
        {
            // If no call is selected, show an informational message
            MessageBox.Show("No call selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Ask for confirmation before assigning the call
        var result = MessageBox.Show($"Do you want to handle call {selectedCall.Id}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // Assign the selected call to the volunteer
                s_bl.Call.AssignCallToVolunteer(Vm.VolunteerId, selectedCall.Id);
                MessageBox.Show($"Call {selectedCall.Id} has been assigned to volunteer {Vm.VolunteerId}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                queryCallList(); // Reload the call list after the assignment
            }
            catch (Exception ex)
            {
                // Display an error message if assigning the call fails
                MessageBox.Show($"Error assigning call to volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Event handler for finishing an ongoing call.
    /// Marks the selected call as finished.
    /// </summary>
    private void FinishCall_Click(object sender, RoutedEventArgs e)
    {
        var call = Vm.Selected; // Get the selected call from the ViewModel
        if (call == null || call.Id == 0)
        {
            // If no ongoing call is selected, show an informational message
            MessageBox.Show("No ongoing call to finish.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            // Close the ongoing call assignment
            s_bl.Call.CloseCallAssignment(Vm.VolunteerId, call.Id);
            MessageBox.Show("The call was marked as closed.");
            queryCallList(); // Reload the call list after the operation
        }
        catch (Exception ex)
        {
            // Display an error message if closing the call assignment fails
            MessageBox.Show($"Error closing the call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Event handler for canceling an ongoing call.
    /// Marks the selected call as canceled.
    /// </summary>
    private void btnCanceled_Click(object sender, RoutedEventArgs e)
    {
        var call = Vm.Selected; // Get the selected call from the ViewModel
        if (call == null || call.Id == 0)
        {
            // If no ongoing call is selected, show an informational message
            MessageBox.Show("No ongoing call to cancel.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            s_bl.Call.CancelCallAssignment(Vm.VolunteerId, call.Id,BO.Role.Volunteer);
            MessageBox.Show("The call was marked as canceled.");
            queryCallList(); // Reload the call list after the operation
        }
        catch (Exception ex)
        {
            // Display an error message if canceling the call fails
            MessageBox.Show($"Error canceling the call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Event handler for the Back button. Closes the current window.
    /// </summary>
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        this.Close(); // Close the window and return to the previous screen
    }
}
