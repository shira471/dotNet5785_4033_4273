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
    public partial class VolunteerWindow : Window, INotifyPropertyChanged
    {
        // public ObservableCollection<BO.OpenCallInList> VolunteerCalls { get; set; } = new ObservableCollection<BO.OpenCallInList>();
        public bool IsVolunteerActive => CurrentVolunteer?.IsActive ?? false;
        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }
        public string? CallDetails { get; set; }
        // Property to check if a call is active
        public bool IsCallActive => !string.IsNullOrEmpty(CallDetails) && CallDetails != "No active call.";

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
            try
            {
                s_bl = BlApi.Factory.Get(); // Factory pattern for BL
            DataContext = this;
            LoadVolunteer(id);
            LoadCallDetails();
                LoadCallDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoudVolunteerWindow(string? id=null) 
        {
            try
            {
                DataContext = this;
                LoadVolunteer(id);
                LoadCallDetails();
            }
            catch(Exception ex) 
            {
                MessageBox.Show($"Error loading Window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadVolunteer(string volunteerId)
        {
            try
            {
                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(int.Parse(volunteerId));
                OnPropertyChanged(nameof(IsVolunteerActive));
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
        private void ActiveCheckBox_Click(object sender, RoutedEventArgs e)
        {
            // אם יש קריאה פעילה, לא לאפשר שינוי למצב "לא פעיל"
            if (IsCallActive)
            {
                MessageBox.Show("Cannot deactivate a volunteer with an active call.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

                // ביטול שינוי המצב ב-CheckBox
                if (sender is CheckBox checkBox)
                {
                    checkBox.IsChecked = true; // להחזיר את ה-CheckBox למצב פעיל
                }
            }
        }
        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // שמירה במערכת ה-BL
                s_bl.Volunteer.UpdateVolunteer(CurrentVolunteer.Id, CurrentVolunteer);

                MessageBox.Show("Volunteer details updated successfully.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                OnPropertyChanged(nameof(IsVolunteerActive));
                string volId = CurrentVolunteer.Id.ToString();
                LoudVolunteerWindow(volId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating volunteer details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FinishCall_Click(object sender, RoutedEventArgs e)
        {
            if (IsCallActive)
            {
                try
                {
                    var callId = int.Parse(CallDetails.Split('\n')[0].Split(':')[1].Trim());
                    s_bl.Call.CloseCallAssignment(CurrentVolunteer.Id, callId);
                    MessageBox.Show("The call was marked as closed.");
                    LoadCallDetails();
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

        private void CancellationCall_Click(object sender, RoutedEventArgs e)
        {
            if (IsCallActive)
            {
                try
                {
                    var callId = int.Parse(CallDetails.Split('\n')[0].Split(':')[1].Trim());
                    s_bl.Call.CancelCallAssignment(CurrentVolunteer.Id, callId);
                    MessageBox.Show("The call was marked as cancelled.");
                    LoadCallDetails();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error ccancelled the call: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No active call to cancelled.");
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

             protected void OnPropertyChanged(string propertyName)
             {
              PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
             }
    }
  }

