using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BO;
using BL.Helpers;
using Helpers;

using DalApi;
namespace PL.Volunteer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public MainWindow()
        {
            InitializeComponent();
            //איתחול ערכים ראשוניים
            //CurrentTime = s_bl.Admin.GetSystemClock();
            //TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            //MaxYearRange = (int)(timeSpan.TotalDays / 365); // המרה לשנים
            //// רישום מתודות ההשקפה
            //s_bl.Admin.AddClockObserver(clockObserver);
            //s_bl.Admin.AddConfigObserver(configObserver);
        }
       
       
        private void OpenListView_Click(object sender, RoutedEventArgs e)
        {
            new VolunteerListWindow().Show();
        }
       
        
        public static readonly DependencyProperty SelectedVolunteerProperty =
    DependencyProperty.Register(
        "SelectedVolunteer",
        typeof(BO.Volunteer),
        typeof(MainWindow),
        new PropertyMetadata(null)
    );
       

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string userId = txtId.Text.Trim().ToLower();
            string password = txtPassword.Text.ToString().ToLower();


            // בדיקת תקינות בסיסית
            if (string.IsNullOrEmpty(userId))
            {
                lblError.Text = "Please enter your ID.";
                lblError.Visibility = Visibility.Visible;
                return;
            }

            // זיהוי סוג המשתמש
            var userType = s_bl.Volunteer.Login(userId, password);

            if (userType == "Manager")
            {
                // מעבר למסך בחירת מנהל
                 new AdminWindow().Show();
                this.Hide();
            }
            else if (userType == "Volunteer")
            {
                // מעבר למסך מתנדב
                new VolunteerWindow(userId).Show();
                this.Hide();
            }
            else
            {
                lblError.Text = "Invalid credentials. Please try again.";
                lblError.Visibility = Visibility.Visible;
            }
        }


        

    }
}