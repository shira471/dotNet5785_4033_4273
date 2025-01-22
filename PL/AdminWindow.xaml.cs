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

namespace PL.Volunteer
{
    /// <summary>
    /// חלון מנהל עיקרי לניהול מתנדבים וקריאות
    /// </summary>
    public partial class AdminWindow : Window
    {
        // גישה לשכבת הביניים (BL)
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public AdminWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            // אתחול שעון המערכת הנוכחי
            CurrentTime = s_bl.Admin.GetSystemClock();

            // חישוב טווח השנים המקסימלי בהתאם למרווח הזמן שבסיכון
            TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            MaxYearRange = (int)(timeSpan.TotalDays / 365); // המרה משנים לימים

            // הוספת מאזינים לעדכוני שעון וקונפיגורציה
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

        // אוסף מתנדבים שמנוהל בחלון
        public ObservableCollection<BO.Volunteer> Volunteers { get; set; }

        /// <summary>
        /// מופעל בעת טעינת החלון
        /// </summary>
        private void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // אתחול מחדש של שעון המערכת וקונפיגורציה
            CurrentTime = s_bl.Admin.GetSystemClock();
            TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            MaxYearRange = (int)(timeSpan.TotalDays / 365);

            // הוספת מאזינים
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

        /// <summary>
        /// מופעל בעת סגירת החלון
        /// </summary>
        private void AdminWindow_Closed(object sender, EventArgs e)
        {
            // הסרת מאזינים למניעת עדכונים מיותרים
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        // מאפיין לתצוגת שעון המערכת
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set
            {
                if (Dispatcher.CheckAccess())
                {
                    SetValue(CurrentTimeProperty, value); // עדכון מתוך Thread של UI
                }
                else
                {
                    Dispatcher.Invoke(() => SetValue(CurrentTimeProperty, value)); // עדכון מתוך Thread אחר
                }
            }
        }

        // הגדרת מאפיין תלוי (DependencyProperty) עבור שעון המערכת
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(
                "CurrentTime",
                typeof(DateTime),
                typeof(AdminWindow),
                new PropertyMetadata(DateTime.Now));

        // מאפיין לטווח השנים המקסימלי
        public int MaxYearRange
        {
            get { return (int)GetValue(MaxYearRangeProperty); }
            set { SetValue(MaxYearRangeProperty, value); }
        }

        // הגדרת מאפיין תלוי עבור טווח השנים המקסימלי
        public static readonly DependencyProperty MaxYearRangeProperty =
            DependencyProperty.Register("MaxYearRange", typeof(int), typeof(MainWindow));

        /// <summary>
        /// מאזין שמעדכן את השעון
        /// </summary>
        private void clockObserver()
        {
            CurrentTime = s_bl.Admin.GetSystemClock();
        }

        /// <summary>
        /// מאזין שמעדכן משתני קונפיגורציה
        /// </summary>
        private void configObserver()
        {
            TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            MaxYearRange = (int)(timeSpan.TotalDays / 365); // המרה משנים לימים
        }

        // עדכון טווח הזמן המקסימלי
        private void UpdateMaxRange_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan timeSpan = TimeSpan.FromDays(MaxYearRange * 365);
            s_bl.Admin.SetRiskTimeSpan(timeSpan);
        }

        public BO.Volunteer SelectedVolunteer { get; set; }

        // הוספת זמן למערכת (דקה, שעה, יום וכו')
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

        /// <summary>
        /// מחיקת מתנדב מהמערכת
        /// </summary>
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
                    s_bl.Volunteer.DeleteVolunteer(SelectedVolunteer.Id); // מחיקת מתנדב ב-BL
                    Volunteers.Remove(Volunteers.First(v => v.Id == SelectedVolunteer.Id)); // מחיקה מהרשימה המקומית
                    MessageBox.Show("המתנדב נמחק בהצלחה.", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"שגיאה במחיקת מתנדב: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ניהול קריאות, ניהול מתנדבים, הדמיית מערכת, אתחול וביצועי בסיס נתונים (המשכים בקוד).
    }
}
