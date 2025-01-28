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
        public ObservableCollection<BO.VolunteerInList> Volunteers { get; set; } = new ObservableCollection<BO.VolunteerInList>();
        public ObservableCollection<BO.CallInList> Calls { get; set; } = new ObservableCollection<BO.CallInList>();

        // public ObservableCollection<BO.OpenCallInList> VolunteerCalls { get; set; } = new ObservableCollection<BO.OpenCallInList>();
        public bool IsVolunteerActive => CurrentVolunteer?.IsActive ?? true;
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
        private void Window_Louded(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer != null)
            {
                s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, VolunteerUpdated);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Remove observers when the window is closed
            if (CurrentVolunteer != null)
            {
                s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, VolunteerUpdated);
            }
            s_bl.Volunteer.RemoveObserver(VolunteerListUpdated);
        }
        private void VolunteerListUpdated()
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // רענון רשימת המתנדבים
                    var volunteers = s_bl.Volunteer.GetVolunteersList();
                    if (volunteers != null)
                    {
                        Volunteers.Clear();
                        foreach (var volunteer in volunteers)
                        {
                            Volunteers.Add(volunteer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating volunteer list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    var updatedVolunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
                    CurrentVolunteer = updatedVolunteer;
                }
            });
        }
        private void VolunteerUpdated()
        {
            Dispatcher.Invoke(() =>
            {
                if (CurrentVolunteer != null)
                {
                    var updatedVolunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
                    CurrentVolunteer = updatedVolunteer;

                }
            });
        }
        private void LoudVolunteerWindow(string? id = null)
        {
            try
            {
                DataContext = this;
                LoadVolunteer(id);
                LoadCallDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadVolunteer(string volunteerId)
        {
            try
            {
                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(int.Parse(volunteerId));
                OnPropertyChanged(nameof(CurrentVolunteer));
                OnPropertyChanged(nameof(IsVolunteerActive));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading volunteer details: {ex.Message}");
                Close();
            }
        }

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
            if (IsCallActive && !CurrentVolunteer.IsActive)
            {
                MessageBox.Show("Cannot deactivate a volunteer with an active call.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                CurrentVolunteer.IsActive = true;
                OnPropertyChanged(nameof(CurrentVolunteer.IsActive));
                return;
            }

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

        private void SelectCall_Click(object sender, RoutedEventArgs e)
        {
            var selectCallWindow = new SelectCallWindow(CurrentVolunteer.Id);
            selectCallWindow.Show();
            LoadCallDetails();
        }
        private void ShowMyCallsHistory_Click(object sender, RoutedEventArgs e)
        {
            var myHistoryWindow = new VolunteerCallsHistoryWindow(CurrentVolunteer.Id);
            myHistoryWindow.Show();
            LoadCallDetails();
        }

        private void CancellationCall_Click(object sender, RoutedEventArgs e)
        {
            if (IsCallActive)
            {
                try
                {
                    var callId = int.Parse(CallDetails.Split('\n')[0].Split(':')[1].Trim());
                    s_bl.Call.CancelCallAssignment(CurrentVolunteer.Id, callId, Role.Volunteer);
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ActiveCheckBox_Checked(object sender, RoutedEventArgs e) { }

        private void ActiveCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsCallActive)
            {
                MessageBox.Show("Cannot deactivate a volunteer with an active call.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (sender is CheckBox checkBox)
                {
                    checkBox.IsChecked = true;
                }
            }
        }
    }
}


