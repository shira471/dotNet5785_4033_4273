using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window, INotifyPropertyChanged
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private static VolunteerListWindow? _volunteerWindow = null;
        private static CallsViewWindow? _callsWindow = null;
        public ObservableCollection<CallInList> CallStatusSummaries { get; set; } = new();
        //public bool IsSimulatorRunning { get; set; }
        public int Interval { get; set; } = 1;
        private bool _isSimulatorRunning;
        public bool IsSimulatorRunning
        {
            get => _isSimulatorRunning;
            set
            {
                _isSimulatorRunning = value;
                OnPropertyChanged(nameof(IsSimulatorRunning)); // עדכון ה-Binding
            }
        }
       

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // DispatcherOperation עבור כל מתודת השקפה
        private volatile DispatcherOperation? _callStatusObserverOperation = null;
        private volatile DispatcherOperation? _clockObserverOperation = null;
        private volatile DispatcherOperation? _configObserverOperation = null;
        public AdminWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            // Initialize the current system clock value
            CurrentTime = s_bl.Admin.GetSystemClock();

            // Calculate the maximum year range based on the risk time span
            TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            MaxYearRange = (int)(timeSpan.TotalDays / 365); // Convert days to years

            // Register clock and configuration observers
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
            LoadCallStatusData();
            s_bl.Call.AddObserver(LoadCallStatusData);
        }

        public ObservableCollection<BO.Volunteer> Volunteers { get; set; }

        /// <summary>
        /// Method triggered when the main screen is loaded
        /// </summary>
        private void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentTime = s_bl.Admin.GetSystemClock();

            TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            MaxYearRange = (int)(timeSpan.TotalDays / 365); // Convert days to years

            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
            s_bl.Call.AddObserver(LoadCallStatusData);
            Application.Current.Dispatcher.Invoke(() => LoadCallStatusData());
        }

        /// <summary>
        /// Method triggered when the main screen is closed
        /// </summary>
        private void AdminWindow_Closed(object sender, EventArgs e)
        {
            if (IsSimulatorRunning)
                s_bl.Admin.StopSimulator();
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
            s_bl.Call.RemoveObserver(LoadCallStatusData);
        }
        private void LoadCallStatusData()
        {
            if (_callStatusObserverOperation is null || _callStatusObserverOperation.Status == DispatcherOperationStatus.Completed)
            {
                _callStatusObserverOperation = Dispatcher.BeginInvoke(() =>
                {
                    // מקבל את כמות הקריאות בכל סטטוס
                    int[] statusCounts = s_bl.Call.GetCallCountsByStatus();

                    // רשימת הסטטוסים מה-Enum
                    var callStatuses = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();

                    // יצירת רשימה של אובייקטים להצגה ב-DataGrid
                    var groupedCalls = callStatuses
                        .Select((status, index) => new CallInList
                        {
                            Status = status,
                            TotalAssignments = statusCounts[index] // משייך את הכמות הנכונה לכל סטטוס
                        })
                        .Where(c => c.TotalAssignments > 0) // מסנן סטטוסים ללא קריאות
                        .ToList();

                    // עדכון ה-ObservableCollection מה-UI Thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CallStatusSummaries.Clear();
                        foreach (var call in groupedCalls)
                        {
                            CallStatusSummaries.Add(call);
                        }
                    });
                });
            }
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCallStatus.SelectedItem is CallInList selectedStatus)
            {
                // שליפת הקריאות לפי הסטטוס שנבחר
                var callsInStatus = s_bl.Call.GetCallsList(CallField.Status, selectedStatus.Status, null).ToList();

                if (callsInStatus.Count == 0)
                {
                    MessageBox.Show("No calls found for this status.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // יצירת הודעה עם כל הקריאות
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.AppendLine($"Calls in {selectedStatus.Status}:");
                messageBuilder.AppendLine(new string('-', 30)); // קו מפריד

                foreach (var call in callsInStatus)
                {
                    messageBuilder.AppendLine($"ID: {call.CallId}");
                    messageBuilder.AppendLine($"Type: {call.CallType}");
                    messageBuilder.AppendLine($"Open Time: {call.OpenTime}");
                    messageBuilder.AppendLine($"Assigned To: {(string.IsNullOrEmpty(call.LastVolunteerName) ? "None" : call.LastVolunteerName)}");
                    messageBuilder.AppendLine($"Total Assignments: {call.TotalAssignments}");
                    messageBuilder.AppendLine(new string('-', 30)); // קו מפריד
                }

                // הצגת כל הקריאות שנמצאו
                MessageBox.Show(messageBuilder.ToString(), $"Calls in {selectedStatus.Status}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set
            {
                if (_clockObserverOperation is null || _clockObserverOperation.Status == DispatcherOperationStatus.Completed)
                {
                    _clockObserverOperation = Dispatcher.BeginInvoke(() => SetValue(CurrentTimeProperty, value));
                }
            }
        }

        private void ToggleSimulator_Click(object sender, RoutedEventArgs e)
        {
            if (IsSimulatorRunning)
            {
                s_bl.Admin.StopSimulator();
                IsSimulatorRunning = false;
            }
            else
            {
                s_bl.Admin.StartSimulator(Interval);
                IsSimulatorRunning = true;
            }
        }

        //public DateTime CurrentTime
        //{
        //    get { return (DateTime)GetValue(CurrentTimeProperty); }
        //    set
        //    {
        //        if (Dispatcher.CheckAccess())
        //        {
        //            SetValue(CurrentTimeProperty, value); // Update from the UI thread
        //        }
        //        else
        //        {
        //            Dispatcher.Invoke(() => SetValue(CurrentTimeProperty, value)); // Update from a different thread
        //        }
        //    }
        //}

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(
                "CurrentTime",
                typeof(DateTime),
                typeof(AdminWindow),
                new PropertyMetadata(DateTime.Now)); // Default value

        public int MaxYearRange
        {
            get { return (int)GetValue(MaxYearRangeProperty); }
            set { SetValue(MaxYearRangeProperty, value); }
        }

        public static readonly DependencyProperty MaxYearRangeProperty =
            DependencyProperty.Register("MaxYearRange", typeof(int), typeof(MainWindow));

        /// <summary>
        /// Observer method to update the clock
        /// </summary>
        private void clockObserver()
        {
            CurrentTime = s_bl.Admin.GetSystemClock();
        }

        /// <summary>
        /// Observer method to update configuration variables
        /// </summary>
        private void configObserver()
        {
            TimeSpan timeSpan = s_bl.Admin.GetRiskTimeSpan();
            MaxYearRange = (int)(timeSpan.TotalDays / 365); // Convert days to years
        }

        private void UpdateMaxRange_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan timeSpan = TimeSpan.FromDays(MaxYearRange * 365); // Convert years to TimeSpan
            s_bl.Admin.SetRiskTimeSpan(timeSpan); // Update configuration via BL
        }

        public BO.Volunteer SelectedVolunteer { get; set; }

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
        /// Deletes a volunteer
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVolunteer == null) // Check if a volunteer is selected
            {
                MessageBox.Show("No volunteer selected for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete volunteer {SelectedVolunteer.FullName}?",
                "Delete Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes) // Proceed with deletion if confirmed
            {
                try
                {
                    s_bl.Volunteer.DeleteVolunteer(SelectedVolunteer.Id); // Delete volunteer in BL
                    Volunteers.Remove(Volunteers.First(v => v.Id == SelectedVolunteer.Id)); // Remove from local list
                    MessageBox.Show("Volunteer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to delete volunteer: {ex.Message}", "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //private void btnCallStatus_Click(object sender, RoutedEventArgs e)
        //{
        //    // קריאה למתודה שמחזירה את מספר הקריאות לפי סטטוסים
        //    int[] temp = s_bl.Call.GetCallCountsByStatus();
        //    // שליפת הסטטוסים על פי סדרם ב-Enum Status
        //    int openCalls = temp[(int)Status.open];
        //    int closeCalls = temp[(int)Status.closed];
        //    int inProgressCalls = temp[(int)Status.inProgres];
        //    int openInRiskCalls = temp[(int)Status.openInRisk];
        //    int expiredCalls = temp[(int)Status.expired];
        //    int closeInRiskCalls = temp[(int)Status.closeInRisk];

        //    // בניית ההודעה להצגה
        //    string message = $"Open calls: {openCalls}\n" +
        //                     $"Close calls: {closeCalls}\n" +
        //                     $"Calls in progress: {inProgressCalls}\n" +
        //                     $"Open in risk calls: {openInRiskCalls}\n" +
        //                     $"Close in risk calls: {closeInRiskCalls}\n" +
        //                     $"Expired Calls: {expiredCalls}";

        //    // הצגת ההודעה
        //    MessageBox.Show(message, "Call Status", MessageBoxButton.OK, MessageBoxImage.Information);
        //}
       
        private void btnCallManage_Click(object sender, RoutedEventArgs e)
        {
            if (_callsWindow == null)
            {
                _callsWindow = new CallsViewWindow();
                _callsWindow.Closed += (s, args) => _callsWindow = null; // מאפסים כשנסגר
                _callsWindow.Show();
            }
            else
            {
                _callsWindow.Focus(); // מביא לחלון קיים
            }
        }

        private void btnVolManage_Click(object sender, RoutedEventArgs e)
        {
            if (_volunteerWindow == null)
            {
                _volunteerWindow = new VolunteerListWindow();
                _volunteerWindow.Closed += (s, args) => _volunteerWindow = null; // מאפסים כשנסגר
                _volunteerWindow.Show();
            }
            else
            {
                _volunteerWindow.Focus(); // מביא לחלון קיים
            }
        }

        private bool _isSimulationRunning = false; // Manage simulation state

        private async void btnStrSimulat_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;  // המרת ה-sender לכפתור

            if (btn == null) return; // אם לא הצלחנו להמיר, לא נמשיך

            if (_isSimulationRunning)
            {
                _isSimulationRunning = false;
                IsSimulatorRunning = false;
                btn.Content = "Start Simulator"; // שינוי הטקסט ל-Start
                MessageBox.Show("The simulator has stopped successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _isSimulationRunning = true;
            IsSimulatorRunning = true;
            btn.Content = "Stop Simulator"; // שינוי הטקסט ל-Stop
            MessageBox.Show("The simulator has started successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            try
            {
                await Task.Run(() =>
                {
                    while (_isSimulationRunning)
                    {
                        s_bl.Admin.AdvanceSystemClock(TimeUnit.Minute); // Advance system clock
                        PerformSimulationLogic();
                        Thread.Sleep(TimeSpan.FromSeconds(5)); // Simulator cycle delay
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during simulation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PerformSimulationLogic()
        {
            var clock = s_bl.Admin.GetSystemClock();
            Console.WriteLine($"Current system clock: {clock}");
        }

        public void SimulationTime_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new VolunteerListWindow();
                window.Show();
                queryVolunteerList(); // Refresh list
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void queryVolunteerList()
        {
            Volunteers.Clear();
            try
            {
                // TODO: Implement volunteer retrieval
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading volunteer list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to initialize the database?",
                                "Confirm Initialization",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this)
                            window.Close();
                    }
                    s_bl.Admin.InitializeDB();
                    // 📌 קריאה ל-Observer כדי לרענן את כל הרכיבים
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadCallStatusData(); // מרענן נתוני קריאות
                        OnPropertyChanged(nameof(CurrentTime)); // עדכון השעון
                        OnPropertyChanged(nameof(CallStatusSummaries)); // עדכון הרשימה
                    });
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ResetDB_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset the database?",
                                "Confirm Reset",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this)
                            window.Close();
                    }
                    s_bl.Admin.ResetDB();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadCallStatusData(); // מרענן נתוני קריאות
                        OnPropertyChanged(nameof(CurrentTime)); // עדכון תצוגת השעון
                        OnPropertyChanged(nameof(CallStatusSummaries)); // עדכון רשימת הקריאות
                    });
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }
    }
}

