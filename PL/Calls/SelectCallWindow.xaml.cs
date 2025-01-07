using BO;
using PL.viewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Calls
{
    /// <summary>
    /// Interaction logic for SelectCallWindow.xaml
    /// </summary>
    public partial class SelectCallWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        // משתנה סטטי עבור BL
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        // מאפיינים עבור הרשימה והמתנדב הנבחר

        public SelectCallWindowVM Vm { get; set; }


        // בנאי החלון
        public SelectCallWindow()
        {
            Vm = new();
            DataContext = Vm;
            InitializeComponent();
            try
            {
                queryCallList(); // טעינת רשימת קריאות ראשונית
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת קריאות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       //פונקציה לטעינת רשימת קריאות
        private void queryCallList()
        {
            Vm.Calls.Clear(); // ניקוי הרשימה
            try
            {
                //var calls = s_bl?.Call.GetCallsList(null, Vm.CallsSortBy) ?? Enumerable.Empty<BO.OpenCallInList>();
                //foreach (var call in calls)
                //{
                //    Vm.Calls.Add(call);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת רשימת הקריאות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // פונקציה לרענון הרשימה כאשר קריאות מתעדכנות
        private void CallListObserver()
        {
            queryCallList();
        }

        // פעולה שמופעלת כשחלון נטען
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                queryCallList(); // טוען את הרשימה
                s_bl?.Call.AddObserver(CallListObserver); //הוספת תצפית על רשימת הקריאות
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
                s_bl?.Volunteer.RemoveObserver(CallListObserver); // מסיר את התצפית על הרשימה
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בסגירת החלון: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnSelected_Click(object sender, RoutedEventArgs e)
        {
            if (Vm.SelectedCall == null)
            {
                MessageBox.Show("לא נבחר קריאה ", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                //לבדוק מה ואיך לעשות
                queryCallList(); // רענון הרשימה
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בבחירת קריאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
