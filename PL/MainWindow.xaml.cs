using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading; // נדרש לעבודה עם Dispatcher

namespace PL.Volunteer
{
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(MainWindow), new PropertyMetadata(1));
        public static readonly DependencyProperty IsSimulatorRunningProperty =
           DependencyProperty.Register("IsSimulatorRunning", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        public int Interval
        {
            get => (int)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        public bool IsSimulatorRunning
        {
            get => (bool)GetValue(IsSimulatorRunningProperty);
            set => SetValue(IsSimulatorRunningProperty, value);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        // Handle the password box password change to bind it to the ViewModel
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowVM viewModel)
            {
                // וידוא שהעדכון קורה בתהליכון הראשי
                Dispatcher.Invoke(() =>
                {
                    viewModel.Password = ((PasswordBox)sender).Password;
                });
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // סוגר את החלון
        }

        // Handle Login button click event
        private void Login_Click(object sender, RoutedEventArgs e)
        {
                if (DataContext is MainWindowVM viewModel)
                {
                    // וידוא שהקריאה ללוגין תתבצע בתהליכון הראשי
                    Dispatcher.Invoke(() =>
                    {
                        viewModel.Login();
                    });
                }
            
        }


        private void AdminWindow_Closed(object sender, EventArgs e)
        {
            if (Application.Current.Windows.OfType<AdminWindow>().Any() == false)
            {
                MainWindowVM.IsManagerLoggedIn = false;
            }
           
        }
    }
}
