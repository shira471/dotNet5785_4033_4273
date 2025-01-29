using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class AddVolunteerWindow : Window
    {
        public ObservableCollection<Role> Roles { get; set; }
        public ObservableCollection<BO.VolunteerInList> Volunteers { get; set; } = new ObservableCollection<BO.VolunteerInList>();
        private volatile DispatcherOperation? _volunteerListUpdateOperation = null;
        private volatile DispatcherOperation? _volunteerUpdateOperation = null;
        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(AddVolunteerWindow), new PropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(VolunteerWindow), new PropertyMetadata("Add"));

        public ObservableCollection<DistanceType> DistanceTypeOptions { get; set; } = new ObservableCollection<DistanceType>(Enum.GetValues(typeof(DistanceType)) as DistanceType[] ?? Array.Empty<DistanceType>());

        private readonly BlApi.IBl s_bl;

        public AddVolunteerWindow(int id = 0)
        {
            InitializeComponent();

            // טען את ערכי ה-Enum
            Roles = new ObservableCollection<Role>(Enum.GetValues(typeof(Role)).Cast<Role>());

            s_bl = BlApi.Factory.Get(); // Factory pattern for BL
            s_bl.Volunteer.AddObserver(VolunteerListUpdated);

            if (id == 0)
            {
                // Add mode: Initialize with default values
                CurrentVolunteer = new BO.Volunteer { Role = Role.Volunteer }; // ברירת מחדל
                ButtonText = "Add";
            }
            else
            {
                // Update mode: Load data from BL
                try
                {
                    CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
                    ButtonText = "Update";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading volunteer: {ex.Message}");
                    Close();
                }
            }
            
            DataContext = this;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // סגור את החלון וחזור לחלון הקודם
            Close();
        }

        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ButtonText == "Add")
                {
                    s_bl.Volunteer.AddVolunteer(CurrentVolunteer!);
                    MessageBox.Show("Volunteer added successfully.");
                }
                else
                {
                    s_bl.Volunteer.UpdateVolunteer(CurrentVolunteer.Id, CurrentVolunteer!);
                    MessageBox.Show("Volunteer updated successfully.");
                }

                // רענון רשימת המתנדבים
                VolunteerListUpdated();

                // סגור את החלון לאחר הצלחה
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void NumericOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
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
            if (CurrentVolunteer != null )
            {
                s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, VolunteerUpdated);
            }
            s_bl.Volunteer.RemoveObserver(VolunteerListUpdated);
        }

        private void VolunteerListUpdated()
        {
            if (_volunteerListUpdateOperation is null || _volunteerListUpdateOperation.Status == DispatcherOperationStatus.Completed)
            {
                _volunteerListUpdateOperation = Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
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
                        if (CurrentVolunteer != null)
                        {
                            var updatedVolunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
                            CurrentVolunteer = updatedVolunteer;
                        }
                    }
                });
            }
          }
        private void VolunteerUpdated()
        {
            if (_volunteerUpdateOperation is null || _volunteerUpdateOperation.Status == DispatcherOperationStatus.Completed)
            {
                _volunteerUpdateOperation = Dispatcher.BeginInvoke(() =>
                {
                    if (CurrentVolunteer != null)
                    {
                        var updatedVolunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
                        CurrentVolunteer = updatedVolunteer;
                    }
                });
            }
        }
    }
}
