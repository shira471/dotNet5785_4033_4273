using System;
using System.Collections.ObjectModel;
using System.Windows;
using BO;
using BlApi;
using PL.Calls;

namespace PL.Volunteer
{
    public partial class VolunteerWindow : Window
    {
        public ObservableCollection<BO.OpenCallInList> VolunteerCalls { get; set; } = new ObservableCollection<BO.OpenCallInList>();

        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }
        public string? CallDetails { get; set; }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

        public ObservableCollection<DistanceType> DistanceTypeOptions { get; set; } = new ObservableCollection<DistanceType>(Enum.GetValues(typeof(DistanceType)) as DistanceType[] ?? Array.Empty<DistanceType>());

        private readonly BlApi.IBl s_bl;
        //public bool IsEditing
        //{
        //    get { return (bool)GetValue(IsEditingProperty); }
        //    set { SetValue(IsEditingProperty, value); }
        //}

        //public static readonly DependencyProperty IsEditingProperty =
        //    DependencyProperty.Register("IsEditing", typeof(bool), typeof(VolunteerWindow), new PropertyMetadata(false));

        public VolunteerWindow(string? id = null)
        {
            InitializeComponent();

            s_bl = BlApi.Factory.Get(); // Factory pattern for BL
            DataContext = this;
            LoadVolunteer(id);
            LoadCallDetails();
        }

        private void LoadVolunteer(string volunteerId)
        {
            try
            {
                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(int.Parse(volunteerId));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading volunteer details: {ex.Message}");
                Close();
            }
        }
        //private void EnableEditing_Click(object sender, RoutedEventArgs e)
        //{
        //    IsEditing = true;
        //}

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // שמירה במערכת ה-BL
                s_bl.Volunteer.UpdateVolunteer(CurrentVolunteer.Id, CurrentVolunteer);

                MessageBox.Show("Volunteer details updated successfully.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);

                // ביטול עריכה לאחר שמירה
               // IsEditing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating volunteer details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FinishCall_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer?.Id != null)
            {
                try
                {
                    s_bl.Call.CloseCallAssignment(CurrentVolunteer.Id, int.Parse(CallDetails));
                    MessageBox.Show("The call was marked as closed.");
                    LoadCallDetails();
                }
                catch
                {
                    MessageBox.Show("Error closing the call. Please try again.");
                }
            }
            else
            {
                MessageBox.Show("No ongoing call to finish.");
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
            myHistoryWindow.ShowDialog();
            LoadCallDetails();
        }

        private void LoadCallDetails()
        {
            // VolunteerCalls = s_bl.Call.GetOpenCallsByVolunteer(CurrentVolunteer.Id, null, null);
        }
    }
}
