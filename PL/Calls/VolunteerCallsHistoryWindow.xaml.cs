using PL.viewModel;
using PL.Volunteer;
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
using System.Windows.Threading;

namespace PL.Calls;

/// <summary>
/// Interaction logic for VolunteerCallsHistoryWindow.xaml
/// </summary>
public partial class VolunteerCallsHistoryWindow : Window
{


    // Reference to the business logic layer
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private volatile DispatcherOperation? _queryClosedCallOperation = null;
    // ViewModel for binding data to the UI
    public VolunteerCallsHistoryVM Vm { get; set; }

    // ID of the volunteer whose call history is displayed
    private int VolunteerId;
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window is VolunteerWindow volunteerWindow) // מחפש את VolunteerWindow בלבד
            {
                volunteerWindow.Show();   // מבטיח שהחלון יהיה גלוי
                volunteerWindow.Activate(); // מביא אותו לקדמת המסך
                this.Close(); // סוגר את החלון הנוכחי
                return;
            }
        }

        // אם VolunteerWindow לא פתוח, ניתן לפתוח אותו מחדש
        var newVolunteerWindow = new VolunteerWindow();
        newVolunteerWindow.Show();
        this.Close();
    }

    /// <summary>
    /// Constructor for the VolunteerCallsHistoryWindow.
    /// Initializes the ViewModel and loads the closed calls.
    /// </summary>
    /// <param name="volunteerId">ID of the volunteer</param>
    public VolunteerCallsHistoryWindow(int volunteerId)
    {
        VolunteerId = volunteerId;
        try { 
        Vm = new VolunteerCallsHistoryVM(volunteerId); // Initialize the ViewModel
        DataContext = Vm; // Set the data context for data binding
        InitializeComponent();

        
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
        if (_queryClosedCallOperation is null || _queryClosedCallOperation.Status == DispatcherOperationStatus.Completed)
        {
            _queryClosedCallOperation = Dispatcher.BeginInvoke(() =>
            {
                Vm.ClosedCalls.Clear(); // ניקוי הרשימה הקיימת
                try
                {
                    var calls = s_bl.Call.GetClosedCallsByVolunteer(VolunteerId, null, null); // שליפת הקריאות הסגורות
                    foreach (var call in calls)
                    {
                        Vm.ClosedCalls.Add(call); // הוספת כל קריאה לרשימה
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading the list of closed calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        s_bl.Call.AddObserver(Vm.UpdateClosedCallsObserver); // רישום המשקיף
    }
    private void Window_Closed(object sender, EventArgs e)
    {
        s_bl.Call.RemoveObserver(Vm.UpdateClosedCallsObserver);
    }
    /// <summary>
    /// Event handler for when the ComboBox selection changes.
    /// Applies filtering logic to the displayed call list.
    /// </summary>
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

}

