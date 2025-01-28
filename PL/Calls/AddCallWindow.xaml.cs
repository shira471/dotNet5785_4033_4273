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
using PL.Volunteer;
using BO;


namespace PL.Call
{
    /// <summary>
    /// Interaction logic for AddCallWindow.xaml
    /// </summary>
    public partial class AddCallWindow : Window
    {
        public ObservableCollection<BO.CallInList> Calls { get; set; } = new ObservableCollection<BO.CallInList>();
        public ObservableCollection<CallType> CallTypes { get; set; }

        public BO.Call? CurrentCall
        {
            get { return (BO.Call?)GetValue(CurrentCallProperty); }
            set { SetValue(CurrentCallProperty, value); }
        }

        public static readonly DependencyProperty CurrentCallProperty =
            DependencyProperty.Register("CurrentCall", typeof(BO.Call), typeof(AddCallWindow), new PropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(AddCallWindow), new PropertyMetadata("Add"));

        private readonly BlApi.IBl s_bl;

        public AddCallWindow(int id = 0)
        {
            InitializeComponent();
            CallTypes = new ObservableCollection<CallType>(Enum.GetValues(typeof(CallType)).Cast<CallType>());

            s_bl = BlApi.Factory.Get(); // Factory pattern for BL
                                        // רישום משקיף לעדכון רשימת הקריאות
                                        // רישום משקיף לעדכון רשימת הקריאות
            s_bl.Call.AddObserver(UpdateCallList);
            if (id == 0)
            {
                // Add mode: Initialize with default values
                CurrentCall = new BO.Call
                {
                    CallType = CallType.Breakfast,            // חמל לארוחת בוקר
                    OpenTime = DateTime.Now
                }; // ברירת מחדל
                ButtonText = "Add";
            }
            else
            {
                // Update mode: Load data from BL
                try
                {
                    CurrentCall = s_bl.Call.GetCallDetails(id.ToString());
                    ButtonText = "Update";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading call: {ex.Message}");
                    Close();
                }
            }

            DataContext = this;
        }

        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ButtonText == "Add")
                {
                    s_bl.Call.AddCall(CurrentCall!);
                    MessageBox.Show("Call added successfully.");
                }
                else
                {
                    s_bl.Call.UpdateCallDetails(CurrentCall);
                  
                    MessageBox.Show("Call updated successfully.");
                }
              
                // סגור את החלון לאחר הצלחה
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
           
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // סגור את החלון וחזור לחלון הקודם
            Close();
        }
        private void Window_Louded(object sender, RoutedEventArgs e)
        {
            if (CurrentCall != null)
            {
                s_bl.Call.AddObserver(CurrentCall.Id, CallUpdated);
            }
        }
        private void CallUpdated()
        {
            Dispatcher.Invoke(() =>
            {
                if (CurrentCall != null)
                {
                    var updatedCall = s_bl.Call.GetCallDetails(CurrentCall.Id.ToString());
                    CurrentCall = updatedCall;

                }
            });
        }
        private void NumericOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Remove observers when the window is closed
            if (CurrentCall != null)
            {
                s_bl.Volunteer.RemoveObserver(CurrentCall.Id, CallUpdated);
            }
            s_bl.Volunteer.RemoveObserver(UpdateCallList);
        }

        private void UpdateCallList()
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // רענון רשימת המתנדבים
                    var calls = s_bl.Call.GetCallsList(null,null,null);
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
                // MessageBox.Show("The call list has been updated!");
            });
        }
    }

}




