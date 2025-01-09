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

        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public SelectCallWindowVM Vm { get; set; }

        private int VolunteerId;
 

        public SelectCallWindow(int volunteerId)
        {
            VolunteerId = volunteerId;

            Vm = new SelectCallWindowVM();
            DataContext = Vm;
            InitializeComponent();
            try
            {
                queryCallList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת קריאות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void queryCallList()
        {
            Vm.Calls.Clear();
            try
            {
                var calls = s_bl.Call.GetOpenCallsByVolunteer(VolunteerId, null,null);
                foreach (var call in calls)
                {
                    Vm.Calls.Add(call);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת רשימת הקריאות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CallListObserver()
        {
            queryCallList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                queryCallList();
                s_bl.Call.AddObserver(CallListObserver);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת החלון: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                s_bl.Call.RemoveObserver(CallListObserver);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בסגירת החלון: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vm.ApplyFilter();
        }

        private void btnSelected_Click(object sender, RoutedEventArgs e)
        {
            if (Vm.SelectedCall == null)
            {
                MessageBox.Show("לא נבחר קריאה", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Adding the selected call to the volunteer's list
                s_bl.Call.AssignCallToVolunteer(VolunteerId, Vm.SelectedCall.Description);

                MessageBox.Show($"קריאה {Vm.SelectedCall.Id} נוספה למתנדב {VolunteerId}", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                queryCallList(); // Refresh the list after assignment
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בהוספת קריאה למתנדב: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
