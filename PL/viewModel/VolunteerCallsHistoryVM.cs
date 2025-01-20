using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PL.viewModel;

public class VolunteerCallsHistoryVM : ViewModelBase
{
    private readonly int volunteerId;
    private readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    public ObservableCollection<ClosedCallInList> ClosedCalls { get; set; } = new ObservableCollection<ClosedCallInList>();

    public IEnumerable<CallType> CallTypes { get; } = Enum.GetValues(typeof(CallType)).Cast<CallType>();
    public IEnumerable<CallField> SortOptions { get; } = Enum.GetValues(typeof(CallField)).Cast<CallField>();

    private CallType? selectedFilterOption;
    public CallType? SelectedFilterOption
    {
        get => selectedFilterOption;
        set
        {
            if (selectedFilterOption != value)
            {
                selectedFilterOption = value;
                OnPropertyChanged(nameof(SelectedFilterOption));
                LoadClosedCalls();
            }
        }
    }

    private CallField? selectedSortOption;
    public CallField? SelectedSortOption
    {
        get => selectedSortOption;
        set
        {
            if (selectedSortOption != value)
            {
                selectedSortOption = value;
                OnPropertyChanged(nameof(SelectedSortOption));
                LoadClosedCalls();
            }
        }
    }

    public VolunteerCallsHistoryVM(int volunteerId)
    {
        this.volunteerId = volunteerId;
        LoadClosedCalls();
    }

    public void LoadClosedCalls()
    {
        ClosedCalls.Clear();
        try
        {
            var calls = s_bl.Call.GetClosedCallsByVolunteer(volunteerId, SelectedFilterOption, SelectedSortOption) ?? Enumerable.Empty<ClosedCallInList>();
            foreach (var call in calls)
            {
                ClosedCalls.Add(call);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading closed calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}