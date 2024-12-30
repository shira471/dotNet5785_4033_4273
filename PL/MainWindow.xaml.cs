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
using PL.Volunteer;
using DalApi;
namespace PL
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
            CurrentTime = s_bl.Admin.GetSystemClock();
            TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            MaxYearRange = (int)(timeSpan.TotalDays / 365); // המרה לשנים
            // רישום מתודות ההשקפה
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }
        /// <summary>
        /// מתודת הטעינה של המסך הראשי
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // השמה של הערך הנוכחי של שעון המערכת
            CurrentTime = s_bl.Admin.GetSystemClock();

            // השמה של ערכי משתני התצורה
            TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            MaxYearRange = (int)(timeSpan.TotalDays / 365); // המרה לשנים

            // הוספת מתודת ההשקפה על השעון כמשקיפה
            s_bl.Admin.AddClockObserver(clockObserver);

            // הוספת מתודת ההשקפה על משתני התצורה כמשקיפה
            s_bl.Admin.AddConfigObserver(configObserver);
        }
        /// <summary>
        /// מתודת הסגירה של המסך הראשי
        /// </summary>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // הסרת מתודת ההשקפה על השעון
            s_bl.Admin.RemoveClockObserver(clockObserver);

            // הסרת מתודת ההשקפה על משתני התצורה
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }
        // הגדרת תכונת תלות עבור CurrentTime
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(
                "CurrentTime",
                typeof(DateTime),
                typeof(MainWindow),
                new PropertyMetadata(DateTime.Now)); // ערך ברירת מחדל
        private void btnAddOneMinute_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.AdvanceSystemClock(BO.TimeUnit.Minute);
        }
        private void btnAddOneHour_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.AdvanceSystemClock(BO.TimeUnit.Hour);
        }
        private void btnAddOneDay_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.AdvanceSystemClock(BO.TimeUnit.Day);
        }
        private void btnAddOneMonth_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.AdvanceSystemClock(BO.TimeUnit.Month);
        }
        private void btnAddOneYear_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.AdvanceSystemClock(BO.TimeUnit.Year);
        }
        public int MaxYearRange
        {
            get { return (int)GetValue(MaxYearRangeProperty); }
            set { SetValue(MaxYearRangeProperty, value); }
        }

        public static readonly DependencyProperty MaxYearRangeProperty =
            DependencyProperty.Register("MaxYearRange", typeof(int), typeof(MainWindow));
        // מתודת השקפה על השעון
        private void clockObserver()
        {
            // עדכון השעון לפי הערך המעודכן ב-BL
            CurrentTime = s_bl.Admin.GetSystemClock();
        }

        // מתודת השקפה על משתני התצורה
        private void configObserver()
        {
            // עדכון הערך של משתני התצורה לפי הערכים המעודכנים ב-BL
            TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            MaxYearRange = (int)(timeSpan.TotalDays / 365); // המרה לשנים
        }
        private void UpdateMaxRange_Click(object sender, RoutedEventArgs e)
        {
            // המרת השנים ל- TimeSpan (לדוגמה, כמה ימים יש בשנתיים)
            TimeSpan timeSpan = TimeSpan.FromDays(MaxYearRange * 365);

            // עדכון ערך משתנה התצורה דרך ה-BL
            s_bl.Admin.SetRiskTimeSpan(timeSpan);
        }
        private void OpenListView_Click(object sender, RoutedEventArgs e)
        {
            new VolunteerListWindow().Show();
        }
        /// <summary>
        /// מתודת אירוע עבור לחיצה על כפתור "Initialize Database"
        /// </summary>
        /// <param name="sender">האובייקט ששלח את האירוע</param>
        /// <param name="e">פרטי האירוע</param>
        private void InitializeDB_Click(object sender, RoutedEventArgs e)
        {
            // הודעת אישור למשתמש
            if (MessageBox.Show("Are you sure you want to initialize the database?",
                                "Confirm Initialization",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    // שינוי האייקון של העכבר לשעון חול
                    Mouse.OverrideCursor = Cursors.Wait;

                    // סגירת כל החלונות הפתוחים חוץ מהחלון הראשי
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this)
                            window.Close();
                    }

                    // קריאה למתודה InitializeDB
                    s_bl.Admin.InitializeDatabase();
                }
                finally
                {
                    // החזרת האייקון של העכבר למצב רגיל
                    Mouse.OverrideCursor = null;
                }
            }
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
            string password = txtPassword.Password.ToString().ToLower();


            // בדיקת תקינות בסיסית
            if (string.IsNullOrEmpty(userId))
            {
                lblError.Text = "Please enter your ID.";
                lblError.Visibility = Visibility.Visible;
                return;
            }

            // זיהוי סוג המשתמש
            var userType = s_bl.Volunteer.Login(userId, password);

            if (userType == "manager")
            {
                // מעבר למסך בחירת מנהל
              //  new AdminWindow().Show();
                this.Hide();
            }
            else if (userType == "volunteer")
            {
                // מעבר למסך מתנדב
                new VolunteerListWindow().Show();
                this.Hide();
            }
            else
            {
                lblError.Text = "Invalid credentials. Please try again.";
                lblError.Visibility = Visibility.Visible;
            }
        }


        /// <summary>
        /// מתודת אירוע עבור לחיצה על כפתור "Reset Database"
        /// </summary>
        /// <param name="sender">האובייקט ששלח את האירוע</param>
        /// <param name="e">פרטי האירוע</param>
        private void ResetDB_Click(object sender, RoutedEventArgs e)
        {
            // הודעת אישור למשתמש
            if (MessageBox.Show("Are you sure you want to reset the database?",
                                "Confirm Reset",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // שינוי האייקון של העכבר לשעון חול
                    Mouse.OverrideCursor = Cursors.Wait;

                    // סגירת כל החלונות הפתוחים חוץ מהחלון הראשי
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this)
                            window.Close();
                    }

                    // קריאה למתודה ResetDB
                    s_bl.Admin.ResetDatabase();
                }
                finally
                {
                    // החזרת האייקון של העכבר למצב רגיל
                    Mouse.OverrideCursor = null;
                }
            }
        }

    }
}