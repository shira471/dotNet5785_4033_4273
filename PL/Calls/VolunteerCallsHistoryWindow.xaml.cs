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
/// </summary>
public partial class VolunteerCallsHistoryWindow : Window, INotifyPropertyChanged
{
    // Event used for property change notifications
    public event PropertyChangedEventHandler? PropertyChanged;

    // Reference to the business logic layer
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // ViewModel for binding data to the UI
    public VolunteerCallsHistoryVM Vm { get; set; }

    // ID of the volunteer whose call history is displayed
    private int VolunteerId;

    /// <summary>
    /// Constructor for the VolunteerCallsHistoryWindow.
    /// Initializes the ViewModel and loads the closed calls.
    /// </summary>
    /// <param name="volunteerId">ID of the volunteer</param>
    public VolunteerCallsHistoryWindow(int volunteerId)
    {
        VolunteerId = volunteerId;

        Vm = new VolunteerCallsHistoryVM(); // Initialize the ViewModel
        DataContext = Vm; // Set the data context for data binding
        InitializeComponent();
   
        try
        {
            queryClosedCallList(); // Load the list of closed calls
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading closed calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Fetches the list of closed calls for the volunteer and updates the ViewModel.
    /// </summary>
    private void queryClosedCallList()
    {
        Vm.ClosedCalls.Clear(); // Clear existing call data in the ViewModel
        try
        {
            var calls = s_bl.Call.GetClosedCallsByVolunteer(VolunteerId, null, null); // Get closed calls from the BL
            foreach (var call in calls)
            {
                Vm.ClosedCalls.Add(call); // Add each call to the ViewModel's collection
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading the list of closed calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Event handler for when the ComboBox selection changes.
    /// Applies filtering logic to the displayed call list.
    /// </summary>
    private void ComboBox_SelectionChanged(object sender, EventArgs e)
    {
        Vm.ApplyFilter(); // Apply filters defined in the ViewModel
    }
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

}
