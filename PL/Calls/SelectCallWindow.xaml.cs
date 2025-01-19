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

            MessageBox.Show($"Error loading call list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

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
        var selected = Vm.SelectedCall;
        if (selected == null)
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
    private void FinishCall_Click(object sender, RoutedEventArgs e)
    {
        var call = Vm.SelectedCall;
        if (call?.Id == null)
        {
            try
            {
                s_bl.Call.CloseCallAssignment(Vm.VolunteerId, call.CallId);
                MessageBox.Show("The call was marked as closed.");
                //LoadCallDetails();
            }
            catch
            {
                MessageBox.Show("Error closing the call. Please try again.");
            }
        }
        else
        {
            MessageBox.Show("No ongoing call to finish.");
        }
    }
    private void btnCanceled_Click(object sender, RoutedEventArgs e)
    {
        var call = Vm.SelectedCall;
        if (call?.Id != null)
        {
            try
            {
                s_bl.Call.CancelCallAssignment(Vm.VolunteerId, call.CallId);
                MessageBox.Show("The call was marked as closed.");
                //LoadCallDetails();
            }
            catch
            {
                MessageBox.Show("Error closing the call. Please try again.");
            }
        }
        else
        {
            MessageBox.Show("No ongoing call to finish.");
        }
    }

}