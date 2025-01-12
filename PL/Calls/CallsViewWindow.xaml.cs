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
using PL.Call;
using PL.Calls;
using PL.viewModel;
using PL.Volunteer;

namespace PL
{
    /// <summary>
    /// Interaction logic for CallsViewWindow.xaml
    /// </summary>
    public partial class CallsViewWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        // משתנה סטטי עבור BL
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        // מאפיינים עבור הרשימה והמתנדב הנבחר

        public CallViewVM vm { get; set; }

        public CallsViewWindow()
        {
            vm = new();
            DataContext = vm;
            InitializeComponent();
            try
            {
                queryCallList(); // טעינת רשימת קריאות ראשונית
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת רשימת הקריאות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // פונקציה לטעינת רשימת המתנדבים
        private void queryCallList()
        {
            vm.Calls.Clear(); // ניקוי הרשימה
            try
            {
                var calls = s_bl?.Call.GetCallsList(null,vm.Calls,null) ?? Enumerable.Empty<BO.CallInList>();
                foreach (var call in calls)
                {
                    vm.Calls.Add(call);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת רשימת המתנדבים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"שגיאה בטעינת החלון: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"שגיאה בהוספת מתנדב: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e) {
            //if (sender is DataGrid dg && dg.SelectedItem is BO.Volunteer s)
            //    SelectedVolunteer = s;
            if (vm.SelectedCall == null)
            {
                MessageBox.Show("לא נבחר מתנדב לעדכון.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {

                var window = new AddCallWindow(vm.SelectedCall.CallId);
                window.ShowDialog();
                queryCallList(); // רענון הרשימה
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בעדכון מתנדב: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    
        private void btnView_Click(object sender, RoutedEventArgs e) {
            if (vm.SelectedCall == null)
            {
                MessageBox.Show("לא נבחר מתנדב לצפייה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                var CallDetails = s_bl.Call.GetCallDetails(vm.SelectedCall.Id.ToString()); // מחיקת המתנדב בלוגיקה העסקית
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
                MessageBox.Show($"לא ניתן לצפות בפרטי המתנדב: {ex.Message}", "שגיאה בצפייה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCall_Click(object sender, RoutedEventArgs e)
        {
            var SelectedCall = vm.SelectedCall;
            if (SelectedCall == null) // בדיקה אם נבחר מתנדב
            {
                MessageBox.Show("לא נבחר מתנדב למחיקה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"האם אתה בטוח שברצונך למחוק את המתנדב {SelectedCall.CallId}?",
                "אישור מחיקה",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes) // ביצוע מחיקה אם המשתמש מאשר
            {
                try
                {
                    s_bl.Volunteer.DeleteVolunteer(SelectedCall.CallId); // מחיקת המתנדב בלוגיקה העסקית
                    vm.Calls.Remove(vm.Calls.First(v => v.Id == SelectedCall.CallId)); // הסרה מהרשימה המקומית
                    MessageBox.Show("המתנדב נמחק בהצלחה.", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"לא ניתן למחוק את המתנדב: {ex.Message}", "שגיאה במחיקה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
