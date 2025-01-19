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
    public ObservableCollection<BO.OpenCallInList> Calls { get; set; }
    private readonly IBl s_bl = Factory.Get(); 

    private BO.OpenCallInList? selectedCall;
 
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
    public SelectCallWindowVM()
    {
        selectedCall=new OpenCallInList();

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
    //    var filteredCalls = new Call().GetOpenCallsByVolunteer(VolunteerId, null, null);
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


