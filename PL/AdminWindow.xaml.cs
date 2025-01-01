using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
//using VolunteerWindow.PL;
namespace PL.Volunteer
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public AdminWindow()
        {
            // השמה של הערך הנוכחי של שעון המערכת
            CurrentTime = s_bl.Admin.GetSystemClock();

            InitializeComponent();
        }
        public ObservableCollection<BO.Volunteer> Volunteers { get; set; }

        public BO.Volunteer SelectedVolunteer { get; set; }

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

        // פעולה למחיקת מתנדב
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVolunteer == null) // בדיקה אם נבחר מתנדב
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

            if (result == MessageBoxResult.Yes) // ביצוע מחיקה אם המשתמש מאשר
            {
                try
                {
                    s_bl.Volunteer.DeleteVolunteer(SelectedVolunteer.Id); // מחיקת המתנדב בלוגיקה העסקית
                    Volunteers.Remove(Volunteers.First(v => v.Id == SelectedVolunteer.Id)); // הסרה מהרשימה המקומית
                    MessageBox.Show("המתנדב נמחק בהצלחה.", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"לא ניתן למחוק את המתנדב: {ex.Message}", "שגיאה במחיקה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        // פעולה להוספת מתנדב חדש
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new VolunteerWindow(); // פתיחת חלון הוספה
                window.ShowDialog();
                queryVolunteerList(); // רענון הרשימה
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בהוספת מתנדב: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // פונקציה לטעינת רשימת המתנדבים
        private void queryVolunteerList()
        {
            Volunteers.Clear(); // ניקוי הרשימה
            try
            {
                var volunteers = s_bl?.Volunteer.GetVolunteersList(null, VolunteerSortBy) ?? Enumerable.Empty<BO.VolunteerInList>();
                foreach (var volunteer in volunteers)
                {
                    Volunteers.Add(volunteer); // הוספת כל מתנדב לרשימה
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת רשימת המתנדבים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
