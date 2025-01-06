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
using BL.Helpers;
using Helpers;
using DalApi;
using System.Xml.Linq;
using PL.viewModel;
using PL.Calls;


namespace PL.Volunteer
{
    public partial class VolunteerWindow : Window
    {
       
        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

        public ObservableCollection<DistanceType> DistanceTypeOptions { get; set; } = new ObservableCollection<DistanceType>(Enum.GetValues(typeof(DistanceType)) as DistanceType[] ?? Array.Empty<DistanceType>());

        private readonly BlApi.IBl s_bl;

        public VolunteerWindow(string? id = null)
        {
            InitializeComponent();

            s_bl = BlApi.Factory.Get(); // Factory pattern for BL
            LoadVolunteer(id);
            LoadCallDetails();
        }

        private void LoadVolunteer(string volunteerId)
        {
            try
            {
                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(int.Parse(volunteerId));

                txtId.Text = volunteerId;
                txtFullName.Text = CurrentVolunteer.FullName;
                txtPhone.Text = CurrentVolunteer.Phone;
                txtEmail.Text = CurrentVolunteer.Email;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת פרטי המתנדב: {ex.Message}");
                Close();
            }
        }
        private void EnableEditing_Click(object sender, RoutedEventArgs e)
        {
            // הפיכת השדות לעריכים
            txtFullName.IsEnabled = true;
            txtPhone.IsEnabled = true;
            txtEmail.IsEnabled = true;

            // הפעלת כפתור השמירה
            btnSave.IsEnabled = true;

            // כיבוי כפתור "עדכון פרטים" בזמן עריכה
            (sender as Button).IsEnabled = false;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // עדכון פרטי המתנדב
                CurrentVolunteer.FullName = txtFullName.Text;
                CurrentVolunteer.Phone = txtPhone.Text;
                CurrentVolunteer.Email = txtEmail.Text;

                // שמירה באמצעות BL
                s_bl.Volunteer.UpdateVolunteer(CurrentVolunteer.Id,CurrentVolunteer);

                MessageBox.Show("פרטי המתנדב נשמרו בהצלחה.", "עדכון", MessageBoxButton.OK, MessageBoxImage.Information);

                // הפיכת השדות ל-ReadOnly מחדש
                txtFullName.IsEnabled = false;
                txtPhone.IsEnabled = false;
                txtEmail.IsEnabled = false;

                // הפעלת כפתור "עדכון פרטים" מחדש
                btnSave.IsEnabled = false;
                EnableEditing_Click(sender: null, e: null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בעדכון פרטי המתנדב: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FinishCall_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer.Id != null)
            {
                try
                {
                    s_bl.Call.CloseCallAssignment(CurrentVolunteer.Id, 1);//לשנות למספר רץ של assigment להבין איך לעשות את זה
                    MessageBox.Show("הקריאה סומנה כסגורה.");
                    LoadCallDetails();
                }
                catch
                {
                    MessageBox.Show("שגיאה בסיום הקריאה. אנא נסה שוב.");
                }
            }
            else
            {
                MessageBox.Show("אין קריאה בטיפול לסיים.");
            }
        }
        private void SelectCall_Click(object sender, RoutedEventArgs e)
        {
            var selectCallWindow = new SelectCallWindow(CurrentVolunteer.Id);
            selectCallWindow.ShowDialog();
            LoadCallDetails();
        }
        private void ShowMyCallsHistory_Click(object sender, RoutedEventArgs e)
        {
            var myHistoryWindow = new VolunteerCallsHistoryWindow(CurrentVolunteer.Id);
            myHistoryWindow.Show();
        }

        private void LoadCallDetails()
        {
            //var currentCall = s_bl.Call.GetCurrentCallForVolunteer(CurrentVolunteer?.Id,1,null);//לבדוק מה זה השלישי ולשנות אותו בהתאם לצורך
            //if (currentCall != null)
            //{
            //    txtCallDetails.Text = $"קריאה: {currentCall.Description}";
            //}
            //else
            //{
            //    txtCallDetails.Text = "אין קריאה בטיפול";
            //}
        }

    }
}
