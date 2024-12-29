using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
        //public BO.Volunteer? SelectedVolunteer { get; set; }
        private BO.Volunteer? selectedVolunteer;
        public BO.Volunteer? SelectedVolunteer
        {
            get => selectedVolunteer;
            set
            {
                selectedVolunteer = value;
                OnPropertyChanged(nameof(SelectedVolunteer)); // יידע את ה-Binding שהערך השתנה
            }
        }

        public BO.VolunteerSortBy VolunteerSortBy { get; set; }

        // בנאי החלון
        public VolunteerListWindow()
        {
            InitializeComponent();
            DataContext = this; 
            var vols = s_bl.Volunteer.GetVolunteersList();
            // מוסיף את הנתונים ל-ObservableCollection
            foreach (var vol in vols)
            {
                Volunteers.Add(vol);
            }
           
        }

        private void queryVolunteerList()
        {
            Volunteers.Clear();
            var volunteers = s_bl?.Volunteer.GetVolunteersList(IsActive, VolunteerSortBy) ?? Enumerable.Empty<BO.VolunteerInList>();
            foreach (var volunteer in volunteers)
            {
                Volunteers.Add(volunteer);
            }
        }

        private void volunteerListObserver()
        {
            queryVolunteerList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            queryVolunteerList(); // טען את הרשימה
            s_bl?.Volunteer.AddObserver(volunteerListObserver); // הרשמה כמשקיף
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Volunteer.RemoveObserver(volunteerListObserver); // הסרת ההשקפה
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // פותח את חלון ההוספה
            var window = new VolunteerWindow();
            window.ShowDialog();

            // רענון הרשימה לאחר הוספה
            queryVolunteerList();
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            
            // בדיקה אם נבחר מתנדב למחיקה
            if (SelectedVolunteer != null)
            {
                // הצגת הודעת אישור למחיקה
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
                        // קריאה לשכבת BL למחיקה
                        s_bl.Volunteer.DeleteVolunteer(SelectedVolunteer.Id);

                        // הסרת המתנדב מהרשימה
                        Volunteers.Remove(Volunteers.First(v => v.Id == SelectedVolunteer.Id));
                    }
                    catch (Exception ex)
                    {
                        // טיפול בחריגה והצגת הודעה
                        MessageBox.Show(
                            $"לא ניתן למחוק את המתנדב: {ex.Message}",
                            "שגיאה במחיקה",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                }
            }
            else
            {
                // הצגת הודעה במקרה שלא נבחר מתנדב
                MessageBox.Show(
                    "לא נבחר מתנדב למחיקה.",
                    "שגיאה",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVolunteer != null)
            {
                var window = new VolunteerWindow(SelectedVolunteer.Id);
                window.ShowDialog();

                // רענון הרשימה לאחר עדכון
                queryVolunteerList();
            }
        }
    }
}