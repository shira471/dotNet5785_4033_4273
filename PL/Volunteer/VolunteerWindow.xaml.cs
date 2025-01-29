using System;
using System.Collections.ObjectModel;
using System.Windows;
using BO;
using BlApi;
using PL.Calls;
using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Claims;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Threading;

using DO;


namespace PL.Volunteer
{
    public partial class VolunteerWindow : Window, INotifyPropertyChanged

    {
        public ObservableCollection<BO.VolunteerInList> Volunteers { get; set; } = new ObservableCollection<BO.VolunteerInList>();
        public ObservableCollection<BO.CallInList> Calls { get; set; } = new ObservableCollection<BO.CallInList>();
        private volatile DispatcherOperation? _volunteerObserverOperation = null;
        private volatile DispatcherOperation? _callObserverOperation = null;

        // public ObservableCollection<BO.OpenCallInList> VolunteerCalls { get; set; } = new ObservableCollection<BO.OpenCallInList>();
        public bool IsVolunteerActive => CurrentVolunteer?.IsActive ?? true;
        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }

        public BO.Call? CurrentCall
        {
            get { return (BO.Call?)GetValue(CurrentCallProperty); }
            set { SetValue(CurrentCallProperty, value); }
        }

        public static readonly DependencyProperty CurrentCallProperty =
          DependencyProperty.Register("CurrentCall", typeof(BO.Call), typeof(VolunteerWindow), new PropertyMetadata(null));



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

                s_bl.Call.AddObserver(LoadCallDetails);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) { 
            if (CurrentVolunteer != null)
            {
                s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, VolunteerUpdated);
                s_bl.Call.AddObserver(CurrentVolunteer.Id, LoadCallDetails);

            }
            if (CurrentCall != null)
            {
                s_bl.Call.AddObserver(CurrentCall.Id, CallUpdated);
            }

            LoadCallDetails();


        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Remove observers when the window is closed
            if (CurrentVolunteer != null)
            {
                s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, VolunteerUpdated);
            }
            s_bl.Volunteer.RemoveObserver(VolunteerListUpdated);
            if (CurrentCall != null)
            {
                s_bl.Call.RemoveObserver(CurrentCall.Id, CallUpdated);
            }
            s_bl.Call.RemoveObserver(CallListUpdated);
        }
        private void CallListUpdated()
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // רענון רשימת המתנדבים
                    var calls = s_bl.Call.GetCallsList(null, null, null);
                    if (calls != null)
                    {
                        Calls.Clear();
                        foreach (var call in calls)
                        {
                            Calls.Add(call);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating volunteer list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    var updatedCall = s_bl.Call.GetCallDetails(CurrentCall.Id.ToString());
                    CurrentCall = updatedCall;
                }
            });
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
            if (_volunteerObserverOperation is null || _volunteerObserverOperation.Status == DispatcherOperationStatus.Completed)
            {
                _volunteerObserverOperation = Dispatcher.BeginInvoke(() =>
                {
                    if (CurrentVolunteer != null)
                    {
                        var updatedVolunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
                        CurrentVolunteer = updatedVolunteer;
                        OnPropertyChanged(nameof(CurrentVolunteer));

                    }
                });
            }
        }
        private void CallUpdated()
        {
            if (_callObserverOperation is null || _callObserverOperation.Status == DispatcherOperationStatus.Completed)
            {
                _callObserverOperation = Dispatcher.BeginInvoke(() =>
                {
                    if (CurrentCall != null)
                    {
                        var updatedCall = s_bl.Call.GetCallDetails(CurrentCall.Id.ToString());
                        CurrentCall = updatedCall;
                        CallDetails = $"ID: {updatedCall.Id}\nDescription: {updatedCall.Description}\nAddress: {updatedCall.Address}";
                        OnPropertyChanged(nameof(CurrentCall));
                        OnPropertyChanged(nameof(CallDetails));


                    }
                });
            }
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
                    s_bl.Call.CancelCallAssignment(CurrentVolunteer.Id, callId, BO.Role.Volunteer);
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
            if (_callObserverOperation is null || _callObserverOperation.Status == DispatcherOperationStatus.Completed)
            {
                _callObserverOperation = Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        var call = s_bl.Call.GetAssignedCallByVolunteer(CurrentVolunteer.Id);
                        if (call != null)
                        {
                            CallDetails = $"ID: {call.Id}\nDescription: {call.Description}\nAddress: {call.Address}";
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
                        MessageBox.Show($"Error loading call details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
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

