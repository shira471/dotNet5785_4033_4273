using PL.viewModel;
using System;
using System.Collections.Generic;
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

namespace PL.Calls;

/// <summary>
/// Interaction logic for VolunteerCallsHistoryWindow.xaml
/// This window displays the history of calls for a specific volunteer.
/// </summary>
public partial class VolunteerCallsHistoryWindow : Window
{
    // Singleton reference to the business logic layer (BL)
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // ViewModel instance for data binding to the UI
    public VolunteerCallsHistoryVM Vm { get; set; }

    // ID of the volunteer whose call history is displayed
    private int VolunteerId;

    /// <summary>
    /// Constructor for the VolunteerCallsHistoryWindow.
    /// Initializes the window, the ViewModel, and loads the closed call history for the volunteer.
    /// </summary>
    /// <param name="volunteerId">ID of the volunteer</param>
    public VolunteerCallsHistoryWindow(int volunteerId)
    {
        VolunteerId = volunteerId; // Store the volunteer ID

        // Initialize the ViewModel with the volunteer's ID
        Vm = new VolunteerCallsHistoryVM(volunteerId);

        // Set the DataContext for data binding to the ViewModel
        DataContext = Vm;

        // Initialize the window's components (UI elements)
        InitializeComponent();

        try
        {
            // Load the list of closed calls for the volunteer
            queryClosedCallList();
        }
        catch (Exception ex)
        {
            // Display an error message if loading the closed calls fails
            MessageBox.Show($"Error loading closed calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Fetches the list of closed calls for the volunteer and updates the ViewModel.
    /// </summary>
    private void queryClosedCallList()
    {
        // Clear the existing call data in the ViewModel
        Vm.ClosedCalls.Clear();

        try
        {
            // Retrieve closed calls for the volunteer from the BL
            var calls = s_bl.Call.GetClosedCallsByVolunteer(VolunteerId, null, null);

            // Add each retrieved call to the ViewModel's ClosedCalls collection
            foreach (var call in calls)
            {
                Vm.ClosedCalls.Add(call);
            }
        }
        catch (Exception ex)
        {
            // Display an error message if retrieving the call list fails
            MessageBox.Show($"Error loading the list of closed calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Event handler for the Back button.
    /// Closes the current window and returns to the previous window.
    /// </summary>
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        this.Close(); // Close the window
    }
}
