using BlApi;
using BO;
using DO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL.viewModel;

    public class SelectCallWindowVM : ViewModelBase
    {
    public ObservableCollection<BO.CallInList> Calls { get; set; }
    private readonly IBl s_bl = Factory.Get();
    // שדה פרטי לאחסון הערך של SelectedCall
    private BO.CallInList? _selectedCall;

    public int VolunteerId { get; }
    public BO.CallInList? SelectedCall
    {
        get => _selectedCall;
        set
        {
            if (_selectedCall != value)
            {
                _selectedCall = value;
                Console.WriteLine($"SelectedCall changed: {_selectedCall?.CallId}"); // לוג לבדיקת שינוי
                OnPropertyChanged(nameof(SelectedCall)); // יידע את ה-Binding שהערך השתנה
            }
        }
    }

    public SelectCallWindowVM(int volunteerId)
    {
        VolunteerId = volunteerId;
        SelectedCall = new CallInList();
        Calls = new(s_bl?.Call.GetCallsList(CallField.Status, Status.Open, null) ?? Enumerable.Empty<BO.CallInList>());
    }
   
}
