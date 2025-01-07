using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL.viewModel;

public class SelectCallWindowVM : ViewModelBase
{
    public ObservableCollection<BO.OpenCallInList> Calls { get; set; } = new ObservableCollection<BO.OpenCallInList>();


    private BO.OpenCallInList? selectedCall;
    public BO.OpenCallInList? SelectedCall
    {
        get => selectedCall;
        set
        {
            if (selectedCall != value)
            {
                selectedCall = value;
                Console.WriteLine($"SelectedCall changed: {selectedCall?.Id}"); // הוספת לוג לבדיקת שינוי
                OnPropertyChanged(nameof(SelectedCall)); // יידע את ה-Binding שהערך השתנה
            }
        }
    }

  //  public BO.CallsSortBy CallsSortBy { get; set; }

    public SelectCallWindowVM()
    {
        selectedCall = new OpenCallInList();
    }

}


