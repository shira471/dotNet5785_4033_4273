using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using BO;
using PL.Call;
using PL.Calls;
using PL.viewModel;
using PL.Volunteer;

namespace PL;

/// <summary>
/// Interaction logic for CallsViewWindow.xaml
/// This window displays a list of calls and provides options to manage them (add, update, delete, cancel).
/// </summary>
public partial class CallsViewWindow : Window
{
    // Business logic interface to interact with the underlying data
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // ViewModel instance for data binding (holds call-related data and logic)
    public CallViewVM vm
    {
        get { return (CallViewVM)GetValue(vmProperty); }
        set { SetValue(vmProperty, value); }
    }

    // DependencyProperty for binding the ViewModel to the window
    public static readonly DependencyProperty vmProperty =
        DependencyProperty.Register("vm", typeof(CallViewVM), typeof(CallsViewWindow));

    /// <summary>
    /// Constructor for initializing the CallsViewWindow.
    /// Initializes the ViewModel and loads calls.
    /// </summary>
    public CallsViewWindow()
    {
        InitializeComponent();
        vm = new CallViewVM(); // Initialize the ViewModel
    }

    /// <summary>
    /// Event handler for the "Add Call" button click.
    /// Opens the window to add a new call and refreshes the call list after adding.
    /// </summary>
    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
        var window = new AddCallWindow(); // Create and show the AddCallWindow
        window.ShowDialog(); // Wait for the dialog to close
        vm.LoadCalls(); // Reload the calls list to include the newly added call
    }

    // Event handlers for other buttons are commented out. 
    // You could use them later if needed for other actions (view, cancel) on calls.

    //private void btnView_Click(object sender, RoutedEventArgs e)
    //{
    //    if (vm.SelectedCall != null)
    //    {
    //        MessageBox.Show($"Viewing details for call {vm.SelectedCall.CallId}", "Call Details");
    //    }
    //}
    //private void btnCancel_Click(object sender, RoutedEventArgs e)
    //{
    //    var call = vm.SelectedCall;
    //    if (call == null || call.Id == 0)
    //    {
    //        MessageBox.Show("No ongoing call to cancel.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
    //        return;
    //    }

    //    try
    //    {
    //        var VolunteerId=s_bl.Volunteer.GetVolunteerForCall(call.CallId);
    //        s_bl.Call.CancelCallAssignment(VolunteerId, call.CallId);
    //        MessageBox.Show("The call was marked as canceled.");
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"Error canceling the call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //    }
    //}

    /// <summary>
    /// Event handler for the "Back" button click.
    /// Closes the current window.
    /// </summary>
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        Close(); // Close the current window
    }

    /// <summary>
    /// Event handler for double-clicking on a call in the data grid.
    /// Opens a window for action selection (Update, Delete, Cancel, View).
    /// </summary>
    private void CallDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (vm.SelectedCall == null)
        {
            // If no call is selected, show an informational message
            MessageBox.Show("No call selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var selectedCall = vm.SelectedCall;

        // Open the ActionSelectionManagerWindow for selecting the action (update, delete, cancel, view)
        var actionWindow = new ActionSelectionManagerWindow("call");
        var result = actionWindow.ShowDialog();

        // Check the result of the action selection and perform the corresponding action
        if (result == true)
        {
            if (actionWindow.IsUpdate) // Update action
            {
                try
                {
                    // Open AddCallWindow in update mode
                    var updateWindow = new AddCallWindow(selectedCall.CallId);
                    updateWindow.ShowDialog(); // Show the update dialog
                    vm.LoadCalls(); // Refresh the call list
                }
                catch (Exception ex)
                {
                    // Show an error message if updating the call fails
                    MessageBox.Show($"Error updating call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (actionWindow.IsDelete) // Delete action
            {
                try
                {
                    // Delete the selected call
                    s_bl.Call.DeleteCall(selectedCall.CallId);
                    vm.Calls.Remove(selectedCall); // Remove the call from the local list
                    MessageBox.Show("Call deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Show an error message if deleting the call fails
                    MessageBox.Show($"Error deleting call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (actionWindow.IsCancel) // Cancel action
            {
                var call = selectedCall;
                if (call == null || call.Id == 0)
                {
                    // Show an error message if no ongoing call is selected
                    MessageBox.Show("No ongoing call to cancel.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                try
                {
                    // Get the volunteer for the call and cancel the call assignment
                    var VolunteerId = s_bl.Volunteer.GetVolunteerForCall(call.CallId);
                    s_bl.Call.CancelCallAssignment(VolunteerId, call.CallId,Role.Manager);
                    MessageBox.Show("The call was marked as canceled.");
                    vm.LoadCalls(); // Refresh the call list
                }
                catch (Exception ex)
                {
                    // Show an error message if canceling the call fails
                    MessageBox.Show($"Error canceling the call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (actionWindow.IsView) // View action
            {
                if (selectedCall != null)
                {
                    // Display the details of the selected call
                    MessageBox.Show($"Viewing details for call {selectedCall.CallId}", "Call Details");
                }
            }
        }
        else if (actionWindow.IsCancel) // Action was canceled
        {
            MessageBox.Show("Action canceled.", "Cancel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
