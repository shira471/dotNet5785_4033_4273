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
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

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
        }

        /// <summary>
        /// Method triggered when the main screen is closed
        /// </summary>
        private void AdminWindow_Closed(object sender, EventArgs e)
        {
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set
            {
                if (Dispatcher.CheckAccess())
                {
                    SetValue(CurrentTimeProperty, value); // Update from the UI thread
                }
                else
                {
                    Dispatcher.Invoke(() => SetValue(CurrentTimeProperty, value)); // Update from a different thread
                }
            }
        }

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

        private void btnCallStatus_Click(object sender, RoutedEventArgs e)
        {
            int[] temp = s_bl.Call.GetCallCountsByStatus();
            int openCalls = temp[4];
            int closeCalls = temp[3];
            int inProgressCalls = temp[2];
            int openInRiskCalls=temp[0];
            int expiredCalls=temp[1];
            int closeInRiskCalls=temp[5];
            string message = $"Open calls: {openCalls}\nClose calls: {closeCalls}\nCalls in progress: {inProgressCalls}\nOpen in risk calls: {openInRiskCalls}\nClose in risk calls: {closeInRiskCalls}\nExpired Calls: {expiredCalls}";
            MessageBox.Show(message, "Call Status", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnCallManage_Click(object sender, RoutedEventArgs e)
        {
            CallsViewWindow cvw = new CallsViewWindow();
            cvw.ShowDialog();
        }

        private void btnVolManage_Click(object sender, RoutedEventArgs e)
        {
            VolunteerListWindow vlw = new VolunteerListWindow();
            vlw.ShowDialog();
        }

        private bool _isSimulationRunning = false; // Manage simulation state

        private async void btnStrSimulat_Click(object sender, RoutedEventArgs e)
        {
            if (_isSimulationRunning)
            {
                MessageBox.Show("The simulator is already running!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isSimulationRunning = true;
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

        private void btnStpSimulat_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSimulationRunning)
            {
                MessageBox.Show("The simulator is already stopped!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isSimulationRunning = false;
            MessageBox.Show("The simulator has stopped successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PerformSimulationLogic()
        {
            var clock = s_bl.Admin.GetSystemClock();
            Console.WriteLine($"Current system clock: {clock}");
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new VolunteerListWindow();
                window.ShowDialog();
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

                    s_bl.Admin.InitializeDatabase();
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

                    s_bl.Admin.ResetDatabase();
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }
    }
}