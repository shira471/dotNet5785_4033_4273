using DalApi;
using DO;
using DalTest;
namespace Dal;

internal class Program
{
    //private static Ivolunteer? s_dalvolunteer = s_dalvolunteer.Create(s_dalvolunteer);
    //private static Icall? s_dalCall = new CallImplementation();
    //private static Iassignment? s_dalAssign = new AssignmentImplementation();
    //private static Iconfig? s_dalConfig = new ConfigImplementation();
     static readonly Idal s_dal = Dallist.intance;  //stage 2
    //private static readonly Idal s_dal = new DalXml(); //stage 3
  
    public static void Main(string[] args)
    {
        try
        {
            // אתחול הנתונים באמצעות המחלקה Initialization
            //Initialization.Do(s_dalAssign, s_dalvolunteer, s_dalCall, s_dalConfig);
            //Initialization.Do(s_dal);//stage 2
            Initialization.Do();//stage 4
            // הצגת התפריט הראשי
            ShowMainMenu();
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות כלליות בתוכנית הראשית
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// מציג את התפריט הראשי ומאפשר גישה לתתי התפריטים
    /// </summary>
    private static void ShowMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Main Menu:");
            Console.WriteLine("1. Show Volunteer Submenu");
            Console.WriteLine("2. Show Call Submenu");
            Console.WriteLine("3. Show Assignment Submenu");
            Console.WriteLine("4. Show Config Submenu");
            Console.WriteLine("5. Exit");
            Console.Write("Please choose an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowVolunteerSubmenu();
                    break;
                case "2":
                    ShowCallSubmenu();
                    break;
                case "3":
                    ShowAssignmentSubmenu();
                    break;
                case "4":
                    ShowConfigSubmenu();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Invalid choice, please try again.");
                    break;
            }
        }
    }

    /// <summary>
    /// מציג את תפריט המשנה למתנדבים ומאפשר לבצע פעולות CRUD
    /// </summary>
    private static void ShowVolunteerSubmenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Volunteer Submenu:");
            Console.WriteLine("1. Add Volunteer");
            Console.WriteLine("2. View Volunteer By ID");
            Console.WriteLine("3. View All Volunteers");
            Console.WriteLine("4. Update Volunteer");
            Console.WriteLine("5. Delete Volunteer");
            Console.WriteLine("6. Delete All Volunteers");
            Console.WriteLine("7. Back to Main Menu");
            Console.Write("Please choose an option: ");

            string choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        AddVolunteer();
                        break;
                    case "2":
                        ViewVolunteerById();
                        break;
                    case "3":
                        ViewAllVolunteers();
                        break;
                    case "4":
                        UpdateVolunteer();
                        break;
                    case "5":
                        DeleteVolunteer();
                        break;
                    case "6":
                        DeleteAllVolunteers();
                        break;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                // טיפול בשגיאות בתפריט המשנה
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// מוסיף מתנדב חדש לרשימה
    /// </summary>
    private static void AddVolunteer()
    {

        Console.WriteLine("Enter Volunteer details:");
        Console.Write("ID: ");
        int id = int.Parse(Console.ReadLine());
        Console.Write("Name: ");
        string name = Console.ReadLine();
        Console.Write("Address: ");
        string address = Console.ReadLine();
        Console.Write("Phone: ");
        string phone= Console.ReadLine();

        var vol = new Volunteer
        {
            idVol = id,
            name = name,
            adress = address,
            phoneNumber = phone
        };

        //s_dalvolunteer?.Create(volunteer);
        s_dal.volunteer.Create(vol);
        Console.WriteLine("Volunteer added successfully.");
    }

    /// <summary>
    /// מציג פרטי מתנדב לפי מזהה
    /// </summary>
    private static void ViewVolunteerById()
    {
        Console.Write("Enter Volunteer ID: ");
        int id = int.Parse(Console.ReadLine());
        var vol = s_dal.volunteer?.Read(id);
        Console.WriteLine(vol != null ? vol.ToString() : "Volunteer not found.");
    }

    /// <summary>
    /// מציג את כל המתנדבים
    /// </summary>
    private static void ViewAllVolunteers()
    {
        var volunteers = s_dal.volunteer?.ReadAll();
        foreach (var vol in volunteers)
        {
            Console.WriteLine(vol);
        }
    }

    ///// <summary>
    ///// מעדכן פרטי מתנדב קיים
    ///// </summary>
    //private static void UpdateVolunteer()
    //{
    //    Console.Write("Enter Volunteer ID to update: ");
    //    int id = int.Parse(Console.ReadLine());
    //    var volunteer = s_dalvolunteer?.Read(id);

    //    if (volunteer != null)
    //    {
    //        Console.WriteLine("Current volunteer data:");
    //        Console.WriteLine(volunteer);

    //        Console.Write("Enter new name (leave empty to keep current): ");
    //        string newName = Console.ReadLine();
    //        if (!string.IsNullOrEmpty(newName))
    //        {
    //            volunteer.name = newName;
    //        }

    //        s_dalvolunteer?.Update(volunteer);
    //        Console.WriteLine("Volunteer updated successfully.");
    //    }
    //    else
    //    {
    //        Console.WriteLine("Volunteer not found.");
    //    }
    //}

    /// <summary>
    /// מוחק מתנדב לפי מזהה
    /// </summary>
    /// 
    /// <summary>
    /// מעדכן פרטי מתנדב קיים
    /// </summary>
    private static void UpdateVolunteer()
    {
        Console.Write("Enter Volunteer ID to update: ");
        int id = int.Parse(Console.ReadLine());
        var vol = s_dal.volunteer?.Read(id);

        if (vol != null)
        {
            Console.WriteLine("Current volunteer data:");
            Console.WriteLine(vol);

            Console.Write("Enter new name (leave empty to keep current): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrEmpty(newName))
            {
                vol = vol with { name = newName }; // שימוש בתכונה "with" כדי ליצור עותק מעודכן
            }

            s_dal.volunteer?.Update(vol);
            Console.WriteLine("Volunteer updated successfully.");
        }
        else
        {
            Console.WriteLine("Volunteer not found.");
        }
    }

    private static void DeleteVolunteer()
    {
        Console.Write("Enter Volunteer ID to delete: ");
        int id = int.Parse(Console.ReadLine());
        s_dal.volunteer?.Delete(id);
        Console.WriteLine("Volunteer deleted successfully.");
    }

    /// <summary>
    /// מוחק את כל המתנדבים
    /// </summary>
    private static void DeleteAllVolunteers()
    {
        s_dal.volunteer?.DeleteAll();
        Console.WriteLine("All volunteers deleted successfully.");
    }

    /// <summary>
    /// מציג את תפריט המשנה לקריאות ומאפשר לבצע פעולות CRUD
    /// </summary>
    private static void ShowCallSubmenu()
    {
        Console.WriteLine("Call submenu - Implement similar to Volunteer.");
    }

    /// <summary>
    /// מציג את תפריט המשנה לשיוכים ומאפשר לבצע פעולות CRUD
    /// </summary>
    private static void ShowAssignmentSubmenu()
    {
        Console.WriteLine("Assignment submenu - Implement similar to Volunteer.");
    }

    /// <summary>
    /// מציג את תפריט המשנה לתצורה ומאפשר לבצע פעולות תצורה
    /// </summary>
    private static void ShowConfigSubmenu()
    {
        Console.WriteLine("Config submenu - Implement specific configuration options.");
    }
}