using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace PL.Volunteer
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        private readonly BlApi.IBl _bl = BlApi.Factory.Get();

        private string _userId;
        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private string _errorVisibility = "Collapsed";
        public string ErrorVisibility
        {
            get => _errorVisibility;
            set
            {
                _errorVisibility = value;
                OnPropertyChanged();
            }
        }

        public void Login()
        {
            try
            {
                ErrorVisibility = "Collapsed";

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
                ErrorVisibility = "Visible";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
