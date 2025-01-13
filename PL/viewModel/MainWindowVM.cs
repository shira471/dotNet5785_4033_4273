using System.Windows;
using System.Windows.Input;

namespace PL.Volunteer
{
    public class MainWindowVM : ViewModelBase
    {
        private readonly BlApi.IBl _bl = BlApi.Factory.Get();

        private string _userId;
        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private bool _isErrorVisible;
        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set
            {
                _isErrorVisible = value;
                OnPropertyChanged(nameof(IsErrorVisible));
            }
        }

        public void Login()
        {
            try
            {
                IsErrorVisible = false;

                var userType = _bl.Volunteer.Login(UserId, Password);

                if (userType == "Manager")
                {
                    new AdminWindow().Show();
                    Application.Current.MainWindow.Close();
                }
                else if (userType == "Volunteer")
                {
                    new VolunteerWindow(UserId).Show();
                    Application.Current.MainWindow.Close();
                }
            }
            catch
            {
                ErrorMessage = "Login failed. Please check your credentials.";
                IsErrorVisible = true;
            }
        }
    }
}