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
    public ObservableCollection<OpenCallInList> Calls { get; set; } = new ObservableCollection<OpenCallInList>();
    public ObservableCollection<CallInList> CallList { get; set; } = new ObservableCollection<CallInList>();

    private OpenCallInList? selectedCall;
    public OpenCallInList? SelectedCall
    {
        get => selectedCall;
        set
        {
            if (selectedCall != value)
            {
                selectedCall = value;
                OnPropertyChanged(nameof(SelectedCall));
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

    //public void LoadCalls(int volunteerId)
    //{
    //    Calls.Clear();
    //    var filteredCalls = new Call().GetOpenCallsByVolunteer(VolunteerId, null,null);
    //    foreach (var call in filteredCalls)
    //    {
    //        Calls.Add(call);
    //    }
    //}

    public void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SelectedFilterOption)) return;

        var filteredCalls = Calls.Where(c => c.Description.Contains(SelectedFilterOption, StringComparison.OrdinalIgnoreCase)).ToList();
        Calls.Clear();
        foreach (var call in filteredCalls)
        {
            Calls.Add(call);
        }
    }
}


