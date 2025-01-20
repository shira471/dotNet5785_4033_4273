using BlApi;
using BO;
using DO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PL.viewModel;

    public class SelectCallWindowVM : ViewModelBase
    {
    private readonly IBl s_bl = Factory.Get();

    public ObservableCollection<BO.OpenCallInList> Calls { get; set; }
    public IEnumerable<BO.CallType> CallTypes { get; } = Enum.GetValues(typeof(BO.CallType)).Cast<BO.CallType>();
    public IEnumerable<BO.SortField> OpenCallSortField { get; } = Enum.GetValues(typeof(SortField)).Cast<BO.SortField>();

    private BO.OpenCallInList? _selectedCall;
    public BO.OpenCallInList? SelectedCall
    {
        get => _selectedCall;
        set
        {
            if (_selectedCall != value)
            {
                _selectedCall = value;
                OnPropertyChanged(nameof(SelectedCall));
            }
        }
    }

    public int VolunteerId { get; }

    private CallType? _filterCallType;
    public CallType? FilterCallType
    {
        get => _filterCallType;
        set
        {
            if (_filterCallType != value)
            {
                _filterCallType = value;
                OnPropertyChanged(nameof(FilterCallType));
                LoadCalls();
            }
        }
    }

    private SortField? _sortField;
    public SortField? SortField
    {
        get => _sortField;
        set
        {
            if (_sortField != value)
            {
                _sortField = value;
                OnPropertyChanged(nameof(SortField));
                LoadCalls();
            }
        }
    }

    public SelectCallWindowVM(int volunteerId)
    {
        VolunteerId = volunteerId;
        Calls = new ObservableCollection<BO.OpenCallInList>();
        LoadCalls();
    }

    public void LoadCalls()
    {
        Calls.Clear();
        try
        {
            var calls = s_bl.Call.GetOpenCallsByVolunteer(VolunteerId, FilterCallType, SortField) ?? Enumerable.Empty<BO.OpenCallInList>();
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
