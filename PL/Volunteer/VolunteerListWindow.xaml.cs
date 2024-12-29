using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;

namespace PL.Volunteer
{
    public partial class VolunteerListWindow : Window
    {
        // משתנה סטטי עבור BL
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // מאפיינים עבור הרשימה והמתנדב הנבחר
        public ObservableCollection<BO.VolunteerInList> Volunteers { get; set; } = new ObservableCollection<BO.VolunteerInList>();
        public BO.Volunteer? SelectedVolunteer { get; set; }
        public BO.VolunteerSortBy VolunteerSortBy { get; set; }

        // בנאי החלון
        public VolunteerListWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void queryVolunteerList()
        {
            Volunteers.Clear();
            var volunteers = s_bl?.Volunteer.GetVolunteersList(true, VolunteerSortBy) ?? Enumerable.Empty<BO.VolunteerInList>();
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