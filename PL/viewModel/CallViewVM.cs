using BlApi;
using BO;
using PL;
using System.Collections.ObjectModel;
using System.Windows;

public class CallViewVM : ViewModelBase
{

    private readonly IBl s_bl = Factory.Get();

    public ObservableCollection<BO.CallInList> Calls { get; set; } = new ObservableCollection<BO.CallInList>();

    public IEnumerable<Status> FilterFields { get; } = Enum.GetValues(typeof(Status)).Cast<Status>();
    public IEnumerable<CallField> SortFields { get; } = Enum.GetValues(typeof(CallField)).Cast<CallField>();

    private Status? selectedFilterField;
    public Status? SelectedFilterField
    {
        get => selectedFilterField;
        set
        {
            if (selectedFilterField != value)
            {
                selectedFilterField = value;
                OnPropertyChanged(nameof(SelectedFilterField));
                LoadCalls();
            }
        }
    }
    private object? selectedFilterValue;
    public object? SelectedFilterValue
    {
        get => selectedFilterValue;
        set
        {
            if (selectedFilterValue != value)
            {
                selectedFilterValue = value;
                OnPropertyChanged(nameof(SelectedFilterValue));
                LoadCalls();
            }
        }
    }

    private CallField? selectedSortField;
    public CallField? SelectedSortField
    {
        get => selectedSortField;
        set
        {
            if (selectedSortField != value)
            {
                selectedSortField = value;
                OnPropertyChanged(nameof(SelectedSortField));
                LoadCalls();
            }
        }
    }

    public BO.CallInList? SelectedCall { get; set; }

    public CallViewVM()
    {
        LoadCalls();
        s_bl.Call.AddObserver(() => LoadCalls()); // הוספת משקיף ישירות ב-ViewModel
    }

    public void LoadCalls()
    {
        try
        {
            var calls = s_bl.Call.GetCallsList(
                        CallField.Status, // סינון לפי `Status`
                        SelectedFilterField, // הערך שנבחר מהפילטר
                        SelectedSortField) // המיון נשאר לפי `CallField`
                   ?? Enumerable.Empty<BO.CallInList>();
            // לוודא שכל העדכונים ל-ObservableCollection מתבצעים ב-Dispatcher
            Application.Current.Dispatcher.Invoke(() =>
            {
                Calls.Clear();
                foreach (var call in calls)
                {
                    Calls.Add(call);
                }
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
