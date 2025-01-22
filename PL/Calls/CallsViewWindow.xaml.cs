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
/// </summary>
public partial class CallsViewWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    public CallViewVM vm
    {
        get { return (CallViewVM)GetValue(vmProperty); }
        set { SetValue(vmProperty, value); }
    }

    public static readonly DependencyProperty vmProperty =
        DependencyProperty.Register("vm", typeof(CallViewVM), typeof(CallsViewWindow));

    public CallsViewWindow()
    {
        InitializeComponent();
        vm = new CallViewVM();
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
        var window = new AddCallWindow();
        window.ShowDialog();
        vm.LoadCalls();
    }

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

    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CallDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (vm.SelectedCall == null)
        {
            MessageBox.Show("No call selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var selectedCall = vm.SelectedCall;

        // פתח את חלון הבחירה
        var actionWindow = new ActionSelectionManagerWindow("call");
        var result = actionWindow.ShowDialog();

        if (result == true)
        {
            if (actionWindow.IsUpdate) // Update
            {
                try
                {
                    var updateWindow = new AddCallWindow(selectedCall.CallId); // חלון לעדכון
                    updateWindow.ShowDialog();
                    vm.LoadCalls(); // רענון הרשימה
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (actionWindow.IsDelete) // Delete
            {
                try
                {
                    s_bl.Call.DeleteCall(selectedCall.CallId); // מחיקת הקריאה
                    vm.Calls.Remove(selectedCall); // הסרת הקריאה מהרשימה המקומית
                    MessageBox.Show("Call deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (actionWindow.IsCancel)
            {
                var call = selectedCall;
                if (call == null || call.Id == 0)
                {
                    MessageBox.Show("No ongoing call to cancel.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                try
                {
                    var VolunteerId = s_bl.Volunteer.GetVolunteerForCall(call.CallId);
                    s_bl.Call.CancelCallAssignment(VolunteerId, call.CallId,Role.Manager);
                    MessageBox.Show("The call was marked as canceled.");
                    vm.LoadCalls(); // רענון הרשימה
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error canceling the call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (actionWindow.IsView)
            {
                if (selectedCall != null)
                {
                    MessageBox.Show($"Viewing details for call {selectedCall.CallId}", "Call Details");
                }
            }
            
        }
        else if (actionWindow.IsCancel) // Cancel
        {
            MessageBox.Show("Action canceled.", "Cancel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}