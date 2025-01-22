using System;
using System.Collections.ObjectModel;
using System.Windows;
using BO;
using BlApi;
using PL.Calls;
using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Controls;

namespace PL.Volunteer
{
    /// <summary>
    /// חלון לניהול פרטי מתנדב הכולל אפשרויות לעריכת פרטי המתנדב,
    /// הצגת שיחות פעילות, סיום שיחות, ביטול שיחות והיסטוריית השיחות.
    /// </summary>
    public partial class VolunteerWindow : Window, INotifyPropertyChanged
    {
        // האם המתנדב פעיל? מוגדר לפי המצב הנוכחי של המתנדב (או ברירת מחדל true אם המתנדב לא נטען).
        public bool IsVolunteerActive => CurrentVolunteer?.IsActive ?? true;

        // המתנדב הנוכחי המוצג בחלון.
        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }

        // פרטי השיחה הפעילה הנוכחית.
        public string? CallDetails { get; set; }

        // האם יש שיחה פעילה? (נבדק לפי תוכן CallDetails).
        public bool IsCallActive => !string.IsNullOrEmpty(CallDetails) && CallDetails != "No active call.";

        // רישום CurrentVolunteer כ-DependencyProperty כדי לאפשר Binding.
        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

        // אפשרויות סוגי מרחק (נטען מתוך enum).
        public ObservableCollection<DistanceType> DistanceTypeOptions { get; set; } =
            new ObservableCollection<DistanceType>(Enum.GetValues(typeof(DistanceType)) as DistanceType[] ?? Array.Empty<DistanceType>());

        // ממשק השירות לניהול לוגיקה עסקית.
        private readonly BlApi.IBl s_bl;

        /// <summary>
        /// קונסטרקטור - טוען נתוני מתנדב על פי מזהה (אם סופק).
        /// </summary>
        /// <param name="id">מזהה המתנדב.</param>
        public VolunteerWindow(string? id = null)
        {
            InitializeComponent();
            try
            {
                s_bl = BlApi.Factory.Get(); // שימוש ב-Factory ליצירת אובייקט ה-BL.
                DataContext = this;
                LoadVolunteer(id); // טעינת פרטי המתנדב.
                LoadCallDetails(); // טעינת פרטי השיחה הפעילה (אם קיימת).
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// טוען את פרטי המתנדב לפי מזהה שסופק.
        /// </summary>
        private void LoadVolunteer(string volunteerId)
        {
            try
            {
                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(int.Parse(volunteerId));
                OnPropertyChanged(nameof(CurrentVolunteer)); // עדכון נתונים ב-UI.
                OnPropertyChanged(nameof(IsVolunteerActive));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading volunteer details: {ex.Message}");
                Close(); // סוגר את החלון אם יש שגיאה.
            }
        }

        /// <summary>
        /// פעולה המונעת שינוי למצב "לא פעיל" אם קיימת שיחה פעילה.
        /// </summary>
        private void ActiveCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (IsCallActive)
            {
                MessageBox.Show("Cannot deactivate a volunteer with an active call.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

                // אם המשתמש ניסה לבטל את ה-CheckBox, מחזירים אותו למצב פעיל.
                if (sender is CheckBox checkBox)
                {
                    checkBox.IsChecked = true;
                }
            }
        }

        /// <summary>
        /// שומר את השינויים שבוצעו בפרטי המתנדב.
        /// </summary>
        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (IsCallActive && !CurrentVolunteer.IsActive)
            {
                MessageBox.Show("Cannot deactivate a volunteer with an active call.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                CurrentVolunteer.IsActive = true;
                OnPropertyChanged(nameof(CurrentVolunteer.IsActive));
                return;
            }

            try
            {
                s_bl.Volunteer.UpdateVolunteer(CurrentVolunteer.Id, CurrentVolunteer); // שמירה במערכת.
                MessageBox.Show("Volunteer details updated successfully.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                OnPropertyChanged(nameof(IsVolunteerActive));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating volunteer details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// מסמן שיחה פעילה כסגורה.
        /// </summary>
        private void FinishCall_Click(object sender, RoutedEventArgs e)
        {
            if (IsCallActive)
            {
                try
                {
                    var callId = int.Parse(CallDetails.Split('\n')[0].Split(':')[1].Trim());
                    s_bl.Call.CloseCallAssignment(CurrentVolunteer.Id, callId); // סגירת השיחה.
                    MessageBox.Show("The call was marked as closed.");
                    LoadCallDetails(); // עדכון נתוני השיחות.
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error closing the call: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No active call to finish.");
            }
        }

        /// <summary>
        /// טוען פרטי השיחה הפעילה של המתנדב.
        /// </summary>
        private void LoadCallDetails()
        {
            try
            {
                var call = s_bl.Call.GetAssignedCallByVolunteer(CurrentVolunteer.Id);
                if (call != null)
                {
                    CallDetails = $"ID: {call.Id}\nDescription: {call.Description}\nAddrees: {call.Address}";
                }
                else
                {
                    CallDetails = "No active call.";
                }

                OnPropertyChanged(nameof(CallDetails));
                OnPropertyChanged(nameof(IsCallActive));
            }
            catch (Exception ex)
            {
                CallDetails = "Error loading call details.";
                MessageBox.Show($"Error loading call details: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// מתריע על שינוי בפרופרטי.
        /// </summary>
        /// <param name="propertyName">שם הפרופרטי.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
