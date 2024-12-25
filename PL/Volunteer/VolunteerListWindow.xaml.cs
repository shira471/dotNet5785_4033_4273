using System;
using System.Collections.Generic;
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
namespace PL.Volunteer
{
    /// <summary>
    /// Interaction logic for VolunteerListWindow.xaml
    /// </summary>
    public partial class VolunteerListWindow : Window
    {
        // משתנה סטטי עבור BL
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public BO.VolunteerSortBy VolunteerSortBy { get; set; } 

        // בנאי החלון
        public VolunteerListWindow()
        {
            InitializeComponent();
        }

        // מאפיין DependencyProperty עבור רשימת מתנדבים
        public IEnumerable<BO.VolunteerInList> VolunteerList
        {
            get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty); }
            set { SetValue(VolunteerListProperty, value); }
        }

        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid != null && dataGrid.SelectedItem != null)
            {
                var selectedVolunteer = dataGrid.SelectedItem as BO.VolunteerInList;
                if (selectedVolunteer != null)
                {
                    MessageBox.Show($"Selected Volunteer: {selectedVolunteer}");
                }
            }
            VolunteerList = s_bl?.Volunteer.GetVolunteersList(IsActive,VolunteerSortBy);
        }
        private void queryVolunteerList()
        {
            VolunteerList = (VolunteerSortBy == null)
                ? s_bl?.Volunteer.GetVolunteersList()!
                : s_bl?.Volunteer.GetVolunteersList(IsActive,VolunteerSortBy)!;
        }
        private void volunteerListObserver()
        {
            queryVolunteerList();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Volunteer.AddObserver(volunteerListObserver); // הרשמה כמשקיף
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Volunteer.RemoveObserver(volunteerListObserver); // הסרת ההשקפה
        }

    }
}