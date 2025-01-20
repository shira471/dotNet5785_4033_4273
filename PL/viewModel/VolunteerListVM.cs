using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PL.viewModel;

public class VolunteerListVM : ViewModelBase
{
    public ObservableCollection<BO.VolunteerInList> Volunteers { get; set; } = new ObservableCollection<BO.VolunteerInList>();

    private BO.VolunteerInList? selectedVolunteer;
    public BO.VolunteerInList? SelectedVolunteer
    {
        get => selectedVolunteer;
        set
        {
            if (selectedVolunteer != value)
            {
                selectedVolunteer = value;
                Console.WriteLine($"SelectedVolunteer changed: {selectedVolunteer?.FullName}"); // הוספת לוג לבדיקת שינוי
                OnPropertyChanged(nameof(SelectedVolunteer)); // יידע את ה-Binding שהערך השתנה
            }
        }
    }

    private string isActiveFilter = "All";
    public string IsActiveFilter
    {
        get => isActiveFilter;
        set
        {
            if (isActiveFilter != value)
            {
                isActiveFilter = value;
                OnPropertyChanged(nameof(IsActiveFilter));
                LoadVolunteerList();
            }
        }
    }
    public BO.VolunteerSortBy VolunteerSortBy { get; set; }

    // הפניה לשכבת ה-BL
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();


    // טעינת המתנדבים מה-BL
    public void LoadVolunteerList()
    {
        Volunteers.Clear();
        try
        {
            // תרגום הפילטר לערכים מתאימים
            bool? isActive = IsActiveFilter switch
            {
                "Active" => true,
                "Inactive" => false,
                _ => null
            };

            var volunteers = s_bl.Volunteer.GetVolunteersList(isActive, VolunteerSortBy) ?? Enumerable.Empty<BO.VolunteerInList>();

            foreach (var volunteer in volunteers)
            {
                Volunteers.Add(volunteer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading volunteer list: {ex.Message}");
        }
    }

    public VolunteerListVM()
    {
        LoadVolunteerList();
    }
}
