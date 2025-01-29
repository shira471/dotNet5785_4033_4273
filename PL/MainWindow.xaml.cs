using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading; // נדרש לעבודה עם Dispatcher

namespace PL.Volunteer
{
    public partial class MainWindow : Window
    {
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
    }
}
