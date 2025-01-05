//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using BO;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PL.viewModel;

//public class VolunteerWindowVM : ViewModelBase
//{
//    public ObservableCollection<BO.VolunteerWindowVM> Volunteers { get; set; } = new ObservableCollection<BO.VolunteerWindowVM>();


//    private BO.VolunteerWindowVM? selectedVolunteer;
//    public BO.VolunteerWindowVM? SelectedVolunteer
//    {
//        get => selectedVolunteer;
//        set
//        {
//            if (selectedVolunteer != value)
//            {
//                selectedVolunteer = value;
//                Console.WriteLine($"SelectedVolunteer changed: {selectedVolunteer?.FullName}"); // הוספת לוג לבדיקת שינוי
//                OnPropertyChanged(nameof(SelectedVolunteer)); // יידע את ה-Binding שהערך השתנה
//            }
//        }
//    }

//    public BO.VolunteerSortBy VolunteerSortBy { get; set; }

//    public VolunteerWindowVM()
//    {
//        selectedVolunteer = new VolunteerWindowVM();
//    }

//}
