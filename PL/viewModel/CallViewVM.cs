using BlApi;
using BO;
using PL;
using System.Collections.ObjectModel;
using System.Windows;

public class CallViewVM : ViewModelBase
{

    private readonly IBl s_bl = Factory.Get();

    public ObservableCollection<BO.CallInList> Calls { get; set; } = new ObservableCollection<BO.CallInList>();

    public IEnumerable<CallField> FilterFields { get; } = Enum.GetValues(typeof(CallField)).Cast<CallField>();
    public IEnumerable<CallField> SortFields { get; } = Enum.GetValues(typeof(CallField)).Cast<CallField>();

    private CallField? selectedFilterField;
    public CallField? SelectedFilterField
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
    }

    public void LoadCalls()
    {
        Calls.Clear();
        try
        {
            var calls = s_bl.Call.GetCallsList(SelectedFilterField, SelectedFilterValue, SelectedSortField) ?? Enumerable.Empty<BO.CallInList>();
            foreach (var call in calls)
            {
                Calls.Add(call);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
