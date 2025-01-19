using BlApi;
using BO;
using PL;
using System.Collections.ObjectModel;

public class CallViewVM : ViewModelBase
{

    public ObservableCollection<BO.CallInList> Calls { get; set; }
    private readonly IBl s_bl = Factory.Get();
    // שדה פרטי לאחסון הערך של SelectedCall
    private BO.CallInList? _selectedCall;

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

    public CallViewVM()
    {
        SelectedCall = new CallInList();
        Calls = new(s_bl?.Call.GetCallsList(null, null, null) ?? Enumerable.Empty<BO.CallInList>());
    }
}
