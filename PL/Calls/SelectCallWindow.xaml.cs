
using BO;
using PL.viewModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Calls;

/// <summary>
/// Interaction logic for SelectCallWindow.xaml
/// </summary>
public partial class SelectCallWindow : Window
{
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

    // Constructor for initializing the window with a specific volunteer ID
    public SelectCallWindow(int volunteerId)
    {
        InitializeComponent();
        try
        {
            Vm = new SelectCallWindowVM(volunteerId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void queryCallList()
    {
        Vm.Calls.Clear();
        try
        {

            // קריאה ל-GetCallsList
            var calls = s_bl?.Call.GetCallsList(CallField.Status, Status.Open, null) ?? Enumerable.Empty<BO.CallInList>();
            foreach (var call in calls)
            {
                Vm.Calls.Add(call);
            }
        }
        catch (Exception ex)
        {
<<<<<<< HEAD
            MessageBox.Show($"Error loading call list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
=======
            if(Vm.Calls!=null)
                Vm.Calls.Clear();
            try
            {
                Vm.Calls = new (s_bl.Call.GetOpenCallsByVolunteer(VolunteerId, null, null));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading the list of calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
>>>>>>> da4dbf356ad3a9b3ae7b5d4ec4b45c08b56ec174
        }
    }

    private void callListObserver()
    {
        queryCallList();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            queryCallList();
            s_bl?.Call.AddObserver(callListObserver);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        try
        {
            s_bl?.Call.RemoveObserver(callListObserver);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error closing window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnSelected_Click(object sender, RoutedEventArgs e)
    {
        if (Vm.SelectedCall == null)
        {
            MessageBox.Show("No call selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            s_bl.Call.AssignCallToVolunteer(Vm.VolunteerId, Vm.SelectedCall.CallId);
            MessageBox.Show($"Call {Vm.SelectedCall.Id} has been assigned to volunteer {Vm.VolunteerId}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            queryCallList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error assigning call to volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

