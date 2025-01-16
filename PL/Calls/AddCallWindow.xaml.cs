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

        public ObservableCollection<CallType> CallTypes { get; set; }
        public BO.Call? CurrentCall
            {
            get { return (BO.Call?)GetValue(CurrentCallProperty); }
            set { SetValue(CurrentCallProperty, value); }
            }

        public static readonly DependencyProperty CurrentCallProperty =
            DependencyProperty.Register("CurrentCall", typeof(BO.Call), typeof(CallsViewWindow), new PropertyMetadata(null));
        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(CallsViewWindow), new PropertyMetadata("Add"));

        private readonly BlApi.IBl s_bl;

        public AddCallWindow(int id = 0)
        {
            InitializeComponent();
            CallTypes = new ObservableCollection<CallType>(Enum.GetValues(typeof(CallType)).Cast<CallType>());

            s_bl = BlApi.Factory.Get(); // Factory pattern for BL

            if (id == 0)
            {
                // Add mode: Initialize with default values
                CurrentCall = new BO.Call { CallType = CallType.Emergency,
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
                    MessageBox.Show($"Error loading volunteer: {ex.Message}");
                    Close();
                }
            }

            DataContext = this;
        }
        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                

                // אם עברנו את כל הבדיקות
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

        private void NumericOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }

}

           
        
  