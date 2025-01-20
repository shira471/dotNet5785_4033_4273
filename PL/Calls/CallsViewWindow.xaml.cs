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
    // משתנה סטטי עבור BL
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    // מאפיינים עבור הרשימה והמתנדב הנבחר

    //public CallViewVM vm { get; set; }


    public CallViewVM vm
    {
        get { return (CallViewVM)GetValue(vmProperty); }
        set { SetValue(vmProperty, value); }
    }

    // Using a DependencyProperty as the backing store for vm.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty vmProperty =
        DependencyProperty.Register("vm", typeof(CallViewVM), typeof(CallsViewWindow));




    public CallsViewWindow()
    {
        InitializeComponent();
        try
        {
            vm = new CallViewVM();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    // פונקציה לטעינת רשימת הקריאות
    private void queryCallList()
    {
        vm.Calls.Clear(); // ניקוי הרשימה
        try
        {
            CallField? filterField = CallField.Status;       // שדה לסינון
            object? filterValue = Status.Open;              // ערך לסינון (קריאות פתוחות)
            CallField? sortField = CallField.AssignedTo;    // שדה למיון

            // קריאה ל-GetCallsList
            var calls = s_bl?.Call.GetCallsList(filterField, filterValue, sortField) ?? Enumerable.Empty<BO.CallInList>();
            foreach (var call in calls)
            {
                vm.Calls.Add(call);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading call list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    // פונקציה לרענון הרשימה כאשר מתנדבים מתעדכנים
    private void callListObserver()
    {
        queryCallList();
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            queryCallList(); // טוען את הרשימה
            s_bl?.Call.AddObserver(callListObserver); // הוספת תצפית על רשימת המתנדבים
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // פעולה שמופעלת כשחלון נסגר
    private void Window_Closed(object sender, EventArgs e)
    {
        try
        {
            s_bl?.Call.RemoveObserver(callListObserver); // מסיר את התצפית על הרשימה
        }
        catch (Exception ex)
        {
            MessageBox.Show($"שגיאה בסגירת החלון: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void btnAdd_Click(object sender, RoutedEventArgs e) {
        try
        {
            var window = new AddCallWindow(); // פתיחת חלון הוספה
            window.ShowDialog();

            queryCallList(); // רענון הרשימה
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    //private void btnUpdate_Click(object sender, RoutedEventArgs e) {

    //    var selectedCall = vm.SelectedCall;
    //    if (selectedCall == null)
    //    {
    //       MessageBox.Show("Please select a call to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    //        return;
    //    }

    //    try
    //    {

    //        var window = new AddCallWindow(vm.SelectedCall.CallId);
    //        window.ShowDialog();
    //        queryCallList(); // רענון הרשימה
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"Error updating call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //    }
    //}

    private void CallDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (vm.SelectedCall == null)
        {
            MessageBox.Show("No call selected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var selectedCall = vm.SelectedCall;

        // פתח את חלון הבחירה
        var actionWindow = new ActionSelectionWindow("call");
        var result = actionWindow.ShowDialog();

        if (result == true)
        {
            if (actionWindow.IsUpdate) // Update
            {
                try
                {
                    var updateWindow = new AddCallWindow(selectedCall.CallId); // חלון לעדכון
                    updateWindow.ShowDialog();
                    queryCallList(); // רענון הרשימה
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
        }
        else if (actionWindow.IsCancel) // Cancel
        {
            MessageBox.Show("Action canceled.", "Cancel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    private void btnView_Click(object sender, RoutedEventArgs e) {
        var selectedCall = vm.SelectedCall;
        if (selectedCall == null)
        {
            MessageBox.Show("Please select a call to view.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            var CallDetails = s_bl.Call.GetCallDetails(selectedCall.CallId.ToString()); // מחיקת המתנדב בלוגיקה העסקית
                                                                            // הצגת התוצאה
                                                                            // בניית מחרוזת להצגה
            string details = $"ID: {CallDetails.Id}\n" +
                             $"Status: {CallDetails.Status}\n" +
                             $"Description: {CallDetails.Description}";

            // הצגת התוצאה
            MessageBox.Show(details, "details", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error viewing call details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    //    private void DeleteCall_Click(object sender, RoutedEventArgs e)
    //    {
    //        var SelectedCall = vm.SelectedCall;

    //        if (SelectedCall == null)
    //        {
    //            MessageBox.Show("Please select a call to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    //            return;
    //        }

    //        var result = MessageBox.Show($"Are you sure you want to delete the call {vm.SelectedCall.CallId}?",
    //                                      "Delete Confirmation",
    //                                      MessageBoxButton.YesNo,
    //                                      MessageBoxImage.Warning);

    //        if (result == MessageBoxResult.Yes)
    //        {
    //            try
    //            {
    //                s_bl.Call.DeleteCall(SelectedCall.CallId);
    //                vm.Calls.Remove(SelectedCall);
    //                MessageBox.Show("Call deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    //            }
    //            catch (Exception ex)
    //            {
    //                MessageBox.Show($"Error deleting call: {ex.Message}", "Error deleting", MessageBoxButton.OK, MessageBoxImage.Error);
    //            }
    //        }
    //    }

    //}
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        // סגור את החלון וחזור לחלון הקודם
        Close();
    }
}