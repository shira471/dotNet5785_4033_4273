using BO;
using PL.viewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Volunteer
{
    // חלון ניהול רשימת מתנדבים
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        // משתנה סטטי עבור BL
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        // מאפיינים עבור הרשימה והמתנדב הנבחר

        public VolunteerListVM vm { get; set; }

        // בנאי החלון
        public VolunteerListWindow()
        {
            vm = new();
            DataContext = vm;
            InitializeComponent();
            try
            {
                queryVolunteerList(); // טעינת רשימת מתנדבים ראשונית
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת רשימת המתנדבים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // פונקציה לטעינת רשימת המתנדבים
        private void queryVolunteerList()
        {
            vm.Volunteers.Clear(); // ניקוי הרשימה
            try
            {
                var volunteers = s_bl?.Volunteer.GetVolunteersList(null, vm.VolunteerSortBy) ?? Enumerable.Empty<BO.VolunteerInList>();
                foreach (var volunteer in volunteers)
                {
                    vm.Volunteers.Add(volunteer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת רשימת המתנדבים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // פונקציה לרענון הרשימה כאשר מתנדבים מתעדכנים
        private void volunteerListObserver()
        {
            queryVolunteerList();
        }

        // פעולה שמופעלת כשחלון נטען
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                queryVolunteerList(); // טוען את הרשימה
                s_bl?.Volunteer.AddObserver(volunteerListObserver); // הוספת תצפית על רשימת המתנדבים
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת החלון: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // פעולה שמופעלת כשחלון נסגר
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                s_bl?.Volunteer.RemoveObserver(volunteerListObserver); // מסיר את התצפית על הרשימה
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בסגירת החלון: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // פעולה להוספת מתנדב חדש
        //private void btnAdd_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var window = new VolunteerWindow(); // פתיחת חלון הוספה
        //        window.ShowDialog();
        //        queryVolunteerList(); // רענון הרשימה
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"שגיאה בהוספת מתנדב: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //// פעולה למחיקת מתנדב
        //private void btnDelete_Click(object sender, RoutedEventArgs e)
        //{
        //    if (SelectedVolunteer == null) // בדיקה אם נבחר מתנדב
        //    {
        //        MessageBox.Show("לא נבחר מתנדב למחיקה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
        //        return;
        //    }

        //    var result = MessageBox.Show(
        //        $"האם אתה בטוח שברצונך למחוק את המתנדב {SelectedVolunteer.FullName}?",
        //        "אישור מחיקה",
        //        MessageBoxButton.YesNo,
        //        MessageBoxImage.Warning
        //    );

        //    if (result == MessageBoxResult.Yes) // ביצוע מחיקה אם המשתמש מאשר
        //    {
        //        try
        //        {
        //            s_bl.Volunteer.DeleteVolunteer(SelectedVolunteer.Id); // מחיקת המתנדב בלוגיקה העסקית
        //            Volunteers.Remove(Volunteers.First(v => v.Id == SelectedVolunteer.Id)); // הסרה מהרשימה המקומית
        //            MessageBox.Show("המתנדב נמחק בהצלחה.", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"לא ניתן למחוק את המתנדב: {ex.Message}", "שגיאה במחיקה", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}
        // פעולה לצפייה בפרטי מתנדב
        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedVolunteer == null)
            {
                MessageBox.Show("לא נבחר מתנדב למחיקה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                var volunteerDetails = s_bl.Volunteer.GetVolunteerDetails(vm.SelectedVolunteer.Id); // מחיקת המתנדב בלוגיקה העסקית
                                                                                                    // הצגת התוצאה
                                                                                                    // בניית מחרוזת להצגה
                string details = $"name: {volunteerDetails.FullName}\n" +
                                 $"phone number: {volunteerDetails.Phone}\n" +
                                 $"role: {volunteerDetails.Role}";

                // הצגת התוצאה
                MessageBox.Show(details, "details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"לא ניתן לצפות בפרטי המתנדב: {ex.Message}", "שגיאה בצפייה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // פעולה לעדכון מתנדב
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //if (sender is DataGrid dg && dg.SelectedItem is BO.Volunteer s)
            //    SelectedVolunteer = s;
            if (vm.SelectedVolunteer == null)
            {
                MessageBox.Show("לא נבחר מתנדב לעדכון.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var window = new VolunteerWindow(vm.SelectedVolunteer.Id);
                window.ShowDialog();
                queryVolunteerList(); // רענון הרשימה
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בעדכון מתנדב: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
