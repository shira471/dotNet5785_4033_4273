using BO;
using PL.viewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Volunteer
{
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

      
        // משתנה סטטי עבור BL
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        // מאפיינים עבור הרשימה והמתנדב הנבחר

        public VolunteerListVM vm {  get; set; }

        // בנאי החלון
        public VolunteerListWindow()
        {
             vm = new ();
            DataContext = vm;
            InitializeComponent();
            try
            {
                queryVolunteerList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת רשימת המתנדבים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void queryVolunteerList()
        {
            vm.Volunteers.Clear();
            try
            {
                var volunteers = s_bl?.Volunteer.GetVolunteersList(null, vm.VolunteerSortBy) ?? Enumerable.Empty<BO.VolunteerInList>();
                foreach (var volunteer in volunteers)
                {
                    vm.Volunteers.Add(volunteer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת רשימת המתנדבים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void volunteerListObserver()
        {
            queryVolunteerList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                queryVolunteerList(); // טען את הרשימה
                s_bl?.Volunteer.AddObserver(volunteerListObserver); // הרשמה כמשקיף
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
                s_bl?.Volunteer.RemoveObserver(volunteerListObserver); // הסרת ההשקפה
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בסגירת החלון: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new VolunteerWindow();
                window.ShowDialog();

                // רענון הרשימה לאחר הוספה
                queryVolunteerList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בהוספת מתנדב: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedVolunteer == null)
            {
                MessageBox.Show("לא נבחר מתנדב למחיקה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"האם אתה בטוח שברצונך למחוק את המתנדב {vm.SelectedVolunteer.FullName}?",
                "אישור מחיקה",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Volunteer.DeleteVolunteer(vm.SelectedVolunteer.Id);
                    vm.Volunteers.Remove(vm.Volunteers.First(v => v.Id == vm.SelectedVolunteer.Id));
                    MessageBox.Show("המתנדב נמחק בהצלחה.", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"לא ניתן למחוק את המתנדב: {ex.Message}", "שגיאה במחיקה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //if (sender is DataGrid dg && dg.SelectedItem is BO.Volunteer s)
            //    SelectedVolunteer = s;
            if (vm.SelectedVolunteer == null)
            {
                MessageBox.Show("לא נבחר מתנדב לעדכון.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var window = new VolunteerWindow(vm.SelectedVolunteer.Id);
                window.ShowDialog();

                // רענון הרשימה לאחר עדכון
                queryVolunteerList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בעדכון מתנדב: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
