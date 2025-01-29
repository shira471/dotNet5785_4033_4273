//using System;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using System.Windows;

//namespace PL.Volunteer
//{
//    public class MainWindowVM 
//    {
//        private readonly BlApi.IBl _bl = BlApi.Factory.Get();
//        private static bool _isManagerLoggedIn = false;
//        private string _userId;
//        public string UserId
//        {
//            get => _userId;
//            set
//            {
//                _userId = value;
//                OnPropertyChanged();
//            }
//        }

//        private string _password;
//        public string Password
//        {
//            get => _password;
//            set
//            {
//                _password = value;
//                OnPropertyChanged();
//            }
//        }

//        private string _errorMessage;
//        public string ErrorMessage
//        {
//            get => _errorMessage;
//            set
//            {
//                _errorMessage = value;
//                OnPropertyChanged();
//            }
//        }

//        private string _errorVisibility = "Collapsed";
//        public string ErrorVisibility
//        {
//            get => _errorVisibility;
//            set
//            {
//                _errorVisibility = value;
//                OnPropertyChanged();
//            }
//        }

//        public void Login()
//        {
//            try
//            {
//                ErrorVisibility = "Collapsed";

//                var userType = _bl.Volunteer.Login(UserId, Password);

//                if (userType == "Manager")
//                {
//                    var result = MessageBox.Show(
//                        "You are logged in as a manager.\n\nClick **YES** to enter Admin Mode.\nClick **NO** to enter as a Volunteer.",
//                        "Manager Login",
//                        MessageBoxButton.YesNo, MessageBoxImage.Question);

//                    if (result == MessageBoxResult.Yes)
//                    {
//                        if (_isManagerLoggedIn)
//                        {
//                            MessageBox.Show("A manager is already logged in. Only one manager can be active at a time.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
//                            return;
//                        }
//                        _isManagerLoggedIn = true;
//                        var adminWindow = new AdminWindow();
//                        adminWindow.Closed += (s, e) => _isManagerLoggedIn = false; // כאשר נסגר – שחרור הדגל
//                        adminWindow.Show();
//                    }
//                    else
//                    {
//                        new VolunteerWindow(UserId).Show();
//                    }
//                }
//                else if (userType == "Volunteer")
//                {
//                    new VolunteerWindow(UserId).Show(); 
//                }

//            }
//            catch(Exception ex) 
//            {
//                var result = MessageBox.Show($"Login failed: {ex.Message}\nWould you like to try again?",
//                 "Login Error",
//                 MessageBoxButton.OKCancel,
//                 MessageBoxImage.Warning);

//                // אם המשתמש לוחץ "Cancel", סוגרים את האפליקציה
//                if (result == MessageBoxResult.Cancel)
//                {
//                    Application.Current.Shutdown();
//                }
//                else
//                {
//                    // המשתמש בחר לנסות שוב – נשאר בחלון
//                    ErrorVisibility = "Visible";
//                }

//            }
//        }

//        public event PropertyChangedEventHandler PropertyChanged;
//        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//    }
//}
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace PL.Volunteer
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        private readonly BlApi.IBl _bl = BlApi.Factory.Get();
        private static bool _isManagerLoggedIn = false;
        public static bool IsManagerLoggedIn
        {
            get => _isManagerLoggedIn;
            set => _isManagerLoggedIn = value;
        }

        private string _userId;
        private string _password;
        private string _errorMessage;
        private string _errorVisibility = "Collapsed";
        private volatile DispatcherOperation? _observerOperation = null; // משמש למניעת קריאות מיותרות ל-Dispatcher

        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public string ErrorVisibility
        {
            get => _errorVisibility;
            set
            {
                if (_errorVisibility != value)
                {
                    _errorVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Login()
        {
            try
            {
                SetErrorVisibility("Collapsed"); // לוודא שהתצוגה מתעדכנת דרך Dispatcher

                var userType = _bl.Volunteer.Login(UserId, Password);

                if (userType == "Manager")
                {
                    ShowManagerLoginDialog();
                }
                else if (userType == "Volunteer")
                {
                    OpenVolunteerWindow();
                }
            }
            catch (Exception ex)
            {
                ShowLoginError(ex.Message);
            }
        }

        private void ShowManagerLoginDialog()
        {
            if (_observerOperation!=null)
            {
                _observerOperation = Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var result = MessageBox.Show(
                        "You are logged in as a manager.\n\nClick **YES** to enter Admin Mode.\nClick **NO** to enter as a Volunteer.",
                        "Manager Login",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        if (Application.Current.Windows.OfType<AdminWindow>().Any())
                        {
                            MessageBox.Show("A manager is already logged in. Only one manager can be active at a time.",
                                "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        //if (_isManagerLoggedIn)
                        //{
                        //    MessageBox.Show("A manager is already logged in. Only one manager can be active at a time.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                        //    return;
                        //}
                        var adminWindow = new AdminWindow();
                        _isManagerLoggedIn = true;
                        adminWindow.Closed += (s, e) => _isManagerLoggedIn = false; // כאשר נסגר – שחרור הדגל
                        adminWindow.Show();
                    }
                    else
                    {
                        OpenVolunteerWindow();
                    }
                });
            }
        }

        private void OpenVolunteerWindow()
        {
            if (_observerOperation != null)
            {
                _observerOperation = Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    new VolunteerWindow(UserId).Show();
                });
            }
        }

        private void ShowLoginError(string message)
        {
            if (_observerOperation is null || _observerOperation.Status == DispatcherOperationStatus.Completed)
            {
                _observerOperation = Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var result = MessageBox.Show(
                        $"Login failed: {message}\nWould you like to try again?",
                        "Login Error",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Cancel)
                    {
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        SetErrorVisibility("Visible");
                    }
                });
            }
        }

        private void SetErrorVisibility(string visibility)
        {
            if (_observerOperation is null || _observerOperation.Status == DispatcherOperationStatus.Completed)
            {
                _observerOperation = Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    ErrorVisibility = visibility;
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
