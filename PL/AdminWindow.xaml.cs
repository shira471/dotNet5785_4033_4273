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
using System.Windows.Threading;
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
        //    // השמה של הערך הנוכחי של שעון המערכת
        //    CurrentTime = s_bl.Admin.GetSystemClock();

           InitializeComponent();
            this.DataContext = this;
            CurrentTime = s_bl.Admin.GetSystemClock();
            TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            MaxYearRange = (int)(timeSpan.TotalDays / 365); // המרה לשנים
            // רישום מתודות ההשקפה
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }
        public ObservableCollection<BO.Volunteer> Volunteers { get; set; }
      

        /// <summary>
        /// מתודת הטעינה של המסך הראשי
        /// </summary>
        private void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //// השמה של הערך הנוכחי של שעון המערכת
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
        private void AdminWindow_Closed(object sender, EventArgs e)
        {
            // הסרת מתודת ההשקפה על השעון
            s_bl.Admin.RemoveClockObserver(clockObserver);

            // הסרת מתודת ההשקפה על משתני התצורה
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }
        // הגדרת תכונת תלות עבור CurrentTime
        //public DateTime CurrentTime
        //{
        //    get { return (DateTime)GetValue(CurrentTimeProperty); }
        //    set { SetValue(CurrentTimeProperty, value); }
        //}
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set
            {
                if (Dispatcher.CheckAccess())
                {
                    SetValue(CurrentTimeProperty, value); // אם הקריאה נעשית מתוך שרשור ה-UI
                }
                else
                {
                    Dispatcher.Invoke(() => SetValue(CurrentTimeProperty, value)); // אם הקריאה נעשית מתוך שרשור אחר
                }
            }
        }


        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(
                "CurrentTime",
                typeof(DateTime),
                typeof(AdminWindow),
                new PropertyMetadata(DateTime.Now)); // ערך ברירת מחדל

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
           // MessageBox.Show($"CurrentTime updated to: {CurrentTime}");
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
        

        public BO.Volunteer SelectedVolunteer { get; set; }

        // הגדרת תכונת תלות עבור CurrentTime
        //public DateTime CurrentTime
        //{
        //    get { return (DateTime)GetValue(CurrentTimeProperty); }
        //    set { SetValue(CurrentTimeProperty, value); }
        //}

        //public static readonly DependencyProperty CurrentTimeProperty =
        //    DependencyProperty.Register(
        //        "CurrentTime",
        //        typeof(DateTime),
        //        typeof(MainWindow),
        //        new PropertyMetadata(DateTime.Now)); // ערך ברירת מחדל
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
        private void btnCallStatus_Click(object sender, RoutedEventArgs e)
        {
            int[] temp = s_bl.Call.GetCallCountsByStatus();
            int openCalls = temp[0];
            int closeCalls = temp[1];
            int inprogressCalls= temp[2];
            string message = $"Open calls: {openCalls}\nClose calls: {closeCalls}\nCalls in progress: {inprogressCalls}";
            MessageBox.Show(message, "Call Status", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void btnCallManage_Click(object sender, RoutedEventArgs e)
        {
            CallsViewWindow cvw= new CallsViewWindow();
            cvw.ShowDialog();
        }
        private void btnVolManage_Click(object sender, RoutedEventArgs e)
        {
            VolunteerListWindow vlw= new VolunteerListWindow();
            vlw.ShowDialog();
        }

        private bool _isSimulationRunning = false; // משתנה לניהול מצב הסימולטור

        private async void btnStrSimulat_Click(object sender, RoutedEventArgs e)
        {
            if (_isSimulationRunning)
            {
                MessageBox.Show("הסימולטור כבר פועל!", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isSimulationRunning = true;

            // התחל סימולטור עם מחזור של 5 דקות
            MessageBox.Show("הסימולטור הופעל בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);

            try
            {
                await Task.Run(() =>
                {
                    while (_isSimulationRunning)
                    {
                        // קידום שעון המערכת
                        s_bl.Admin.AdvanceSystemClock(TimeUnit.Minute);

                        // הוספת לוגיקה נוספת אם נדרש
                        PerformSimulationLogic();

                        // המתנה של 5 שניות (לדוגמה, מחזור הסימולטור)
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה במהלך פעולת הסימולטור: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // פונקציה לעצירת הסימולטור
        private void btnStpSimulat_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSimulationRunning)
            {
                MessageBox.Show("הסימולטור כבר כבוי!", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isSimulationRunning = false;
            MessageBox.Show("הסימולטור הופסק בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // לוגיקה נוספת של הסימולטור
        private void PerformSimulationLogic()
        {
            // דוגמה לפעולה נוספת
            var clock = s_bl.Admin.GetSystemClock();
            Console.WriteLine($"Current system clock: {clock}");
        }

        // פעולה להוספת מתנדב חדש
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new VolunteerListWindow(); // פתיחת חלון הוספה
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
                //TODO
                // var volunteers = s_bl?.Volunteer.GetVolunteersList(null, null) ?? Enumerable.Empty<BO.VolunteerInList>();
                //foreach (var volunteer in volunteers)
                {
                    //TODO
                    //Volunteers.Add(volunteer); // הוספת כל מתנדב לרשימה
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
