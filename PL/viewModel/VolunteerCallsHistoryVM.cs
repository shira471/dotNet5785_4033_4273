using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL.viewModel;

public class VolunteerCallsHistoryVM : ViewModelBase
{
    public ObservableCollection<ClosedCallInList> ClosedCalls { get; set; } = new ObservableCollection<ClosedCallInList>();

    private ClosedCallInList? selectedClosedCall;
    public ClosedCallInList? SelectedClosedCall
    {
        get => selectedClosedCall;
        set
        {
            if (selectedClosedCall != value)
            {
                selectedClosedCall = value;
                OnPropertyChanged(nameof(SelectedClosedCall));
            }
        }
    }

    private string? selectedFilterOption;
    public string? SelectedFilterOption
    {
        get => selectedFilterOption;
        set
        {
            if (selectedFilterOption != value)
            {
                selectedFilterOption = value;
                ApplyFilter();
                OnPropertyChanged(nameof(SelectedFilterOption));
            }
        }
    }

    //public void LoadClosedCalls(int volunteerId, Enum? callType = null, Enum? sortField = null)
    //{
    //    ClosedCalls.Clear();
    //    var filteredClosedCalls = new BL().GetClosedCallsByVolunteer(volunteerId, callType, sortField);
    //    foreach (var call in filteredClosedCalls)
    //    {
    //        ClosedCalls.Add(call);
    //    }
    //}

    public void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SelectedFilterOption)) return;

        var filteredClosedCalls = ClosedCalls.Where(c => c.Address.Contains(SelectedFilterOption, StringComparison.OrdinalIgnoreCase)).ToList();
        ClosedCalls.Clear();
        foreach (var call in filteredClosedCalls)
        {
            ClosedCalls.Add(call);
        }
    }
}