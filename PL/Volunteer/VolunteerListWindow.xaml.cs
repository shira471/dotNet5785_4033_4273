using BO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Volunteer
{
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // משתנה סטטי עבור BL
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        // מאפיינים עבור הרשימה והמתנדב הנבחר
        public ObservableCollection<BO.VolunteerInList> Volunteers { get; set; } = new ObservableCollection<BO.VolunteerInList>();


        private BO.VolunteerInList? selectedVolunteer;
        public BO.VolunteerInList? SelectedVolunteer
        {
            get => selectedVolunteer;
            set
            {
                if (selectedVolunteer != value)
                {
                    selectedVolunteer = value;
                    Console.WriteLine($"SelectedVolunteer changed: {selectedVolunteer?.FullName}"); // הוספת לוג לבדיקת שינוי
                    OnPropertyChanged(nameof(SelectedVolunteer)); // יידע את ה-Binding שהערך השתנה
                }
            }
        }

        public BO.VolunteerSortBy VolunteerSortBy { get; set; }

        // בנאי החלון
        public VolunteerListWindow()
        {
            InitializeComponent();
            selectedVolunteer = new VolunteerInList();
            DataContext = this;
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
            Volunteers.Clear();
            try
            {
                var volunteers = s_bl?.Volunteer.GetVolunteersList(null, VolunteerSortBy) ?? Enumerable.Empty<BO.VolunteerInList>();
                foreach (var volunteer in volunteers)
                {
                    Volunteers.Add(volunteer);
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
            if (SelectedVolunteer == null)
            {
                MessageBox.Show("לא נבחר מתנדב למחיקה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"האם אתה בטוח שברצונך למחוק את המתנדב {SelectedVolunteer.FullName}?",
                "אישור מחיקה",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Volunteer.DeleteVolunteer(SelectedVolunteer.Id);
                    Volunteers.Remove(Volunteers.First(v => v.Id == SelectedVolunteer.Id));
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
            if (SelectedVolunteer == null)
            {
                MessageBox.Show("לא נבחר מתנדב לעדכון.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var window = new VolunteerWindow(SelectedVolunteer.Id);
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
