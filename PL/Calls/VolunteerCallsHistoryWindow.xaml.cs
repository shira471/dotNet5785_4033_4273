using PL.viewModel;
using System;
using System.Collections.Generic;
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

namespace PL.Calls
{
    /// <summary>
    /// Interaction logic for VolunteerCallsHistoryWindow.xaml
    /// </summary>
    public partial class VolunteerCallsHistoryWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public VolunteerCallsHistoryVM Vm { get; set; }

        private int VolunteerId;

        public VolunteerCallsHistoryWindow(int volunteerId)
        {
            VolunteerId = volunteerId;

            Vm = new VolunteerCallsHistoryVM();
            DataContext = Vm;
            InitializeComponent();
            try
            {
                queryClosedCallList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת קריאות סגורות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void queryClosedCallList()
        {
            Vm.ClosedCalls.Clear();
            try
            {
                var calls = s_bl.Call.GetClosedCallsByVolunteer(VolunteerId, null, null);
                foreach (var call in calls)
                {
                    Vm.ClosedCalls.Add(call);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת רשימת הקריאות הסגורות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            Vm.ApplyFilter();
        }
    }
}
