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
using BO;

namespace PL.Volunteer
{
    // A window for adding or updating volunteer details
    public partial class AddVolunteerWindow : Window
    {
        // Observable collection to store available roles for volunteers
        public ObservableCollection<Role> Roles { get; set; }

        // Dependency property to hold the currently edited volunteer
        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

        // Dependency property to dynamically set the text on the button ("Add" or "Update")
        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(VolunteerWindow), new PropertyMetadata("Add"));

        // Observable collection to store distance type options from an enum
        public ObservableCollection<DistanceType> DistanceTypeOptions { get; set; } =
            new ObservableCollection<DistanceType>(Enum.GetValues(typeof(DistanceType)) as DistanceType[] ?? Array.Empty<DistanceType>());

        // Reference to the business logic layer (BL)
        private readonly BlApi.IBl s_bl;

        // Constructor for initializing the AddVolunteerWindow
        // `id` is used to determine if the window is in "Add" or "Update" mode
        public AddVolunteerWindow(int id = 0)
        {
            InitializeComponent();

            // Load all possible roles from the Role enum
            Roles = new ObservableCollection<Role>(Enum.GetValues(typeof(Role)).Cast<Role>());

            s_bl = BlApi.Factory.Get(); // Factory pattern to get the BL instance

            if (id == 0)
            {
                // Add mode: Initialize with default values
                CurrentVolunteer = new BO.Volunteer { Role = Role.Volunteer }; // Default role is "Volunteer"
                ButtonText = "Add";
            }
            else
            {
                // Update mode: Load volunteer data from the BL using the provided ID
                try
                {
                    CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
                    ButtonText = "Update";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading volunteer: {ex.Message}");
                    Close(); // Close the window if an error occurs
                }
            }

            DataContext = this; // Set the DataContext to bind data to the UI
        }

        // Event handler for the "Back" button
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Close the window and return to the previous window
            Close();
        }

        // Event handler for the "Add/Update" button
        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ButtonText == "Add") // Check if the action is "Add"
                {
                    // Add the new volunteer to the system
                    s_bl.Volunteer.AddVolunteer(CurrentVolunteer!);
                    MessageBox.Show("Volunteer added successfully.");
                }
                else // Action is "Update"
                {
                    // Update the existing volunteer's details
                    s_bl.Volunteer.UpdateVolunteer(CurrentVolunteer.Id, CurrentVolunteer!);
                    MessageBox.Show("Volunteer updated successfully.");
                }

                // Close the window after the operation succeeds
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}"); // Display an error message
            }
        }

        // Event handler to ensure only numeric input is accepted (for specific fields like phone numbers or IDs)
        private void NumericOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only numeric input by trying to parse the input as an integer
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}
