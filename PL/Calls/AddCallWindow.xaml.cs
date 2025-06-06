﻿using System;
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
using System.Windows.Threading;


namespace PL.Call
{
    /// <summary>
    /// Interaction logic for AddCallWindow.xaml
    /// </summary>
    public partial class AddCallWindow : Window
    {
        public ObservableCollection<BO.CallInList> Calls { get; set; } = new ObservableCollection<BO.CallInList>();
        public ObservableCollection<CallType> CallTypes { get; set; }
        private volatile DispatcherOperation? _updateCallListOperation = null;
        private volatile DispatcherOperation? _callUpdatedOperation = null;
        
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
            DateTime systemClock = s_bl.Admin.GetSystemClock();
            s_bl.Call.AddObserver(UpdateCallList);
            if (id == 0)
            {
                // Add mode: Initialize with default values
                CurrentCall = new BO.Call
                {
                    CallType = CallType.Breakfast,            // חמל לארוחת בוקר
                    OpenTime = systemClock,
                    MaxEndTime = systemClock.AddDays(1),
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
        private void MaxEndTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCall?.MaxEndTime == null)
                return;

            if (sender is TextBox textBox)
            {
                if (TimeSpan.TryParse(textBox.Text, out TimeSpan newTime))
                {
                    DateTime currentDate = CurrentCall.MaxEndTime.Value.Date; // שומר את התאריך
                    CurrentCall.MaxEndTime = currentDate.Add(newTime);
                }
            }
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
            foreach (Window window in Application.Current.Windows)
            {
                if (window is CallsViewWindow callsViewWindow) // מחפש את CallsViewWindow בלבד
                {
                    callsViewWindow.Show();   // מבטיח שהחלון יהיה גלוי
                    callsViewWindow.Activate(); // מביא אותו לקדמת המסך
                    this.Close(); // סוגר את VolunteerListWindow
                    return;
                }
            }

            // אם לא נמצא CallsViewWindow, ניתן לפתוח אותו מחדש
            var newCallsViewWindow = new CallsViewWindow();
            newCallsViewWindow.Show();
            this.Close();
        }

        private void Window_Louded(object sender, RoutedEventArgs e)
        {
            if (CurrentCall != null)
            {
                s_bl.Call.AddObserver(CurrentCall.Id, CallUpdated);
            }
            s_bl.Call.AddObserver(UpdateCallList);
        }
        private void CallUpdated()
        {
            if (_callUpdatedOperation is null || _callUpdatedOperation.Status == DispatcherOperationStatus.Completed)
            {
                _callUpdatedOperation = Dispatcher.BeginInvoke(() =>
                {
                    if (CurrentCall != null)
                    {
                        var updatedCall = s_bl.Call.GetCallDetails(CurrentCall.Id.ToString());
                        CurrentCall = updatedCall;
                    }
                });
            }
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
            if (_updateCallListOperation is null || _updateCallListOperation.Status == DispatcherOperationStatus.Completed)
            {
                _updateCallListOperation = Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
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
                        MessageBox.Show($"Error updating call list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (CurrentCall != null)
                        {
                            var updatedCall = s_bl.Call.GetCallDetails(CurrentCall.Id.ToString());
                            CurrentCall = updatedCall;
                        }
                    }
                });
            }
        }

    }
}




