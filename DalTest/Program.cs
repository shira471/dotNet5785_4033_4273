//using Dal;
//using DalApi;
//using DO;

//namespace DalTest
//{
//    internal class Program
//    {
//        // שדות סטטיים עבור הממשקים
//        private static Ivolunteer? s_dalvolunteer = new VolunteerImplementation(); //stage 1
//        private static Icall? s_dalCall = new CallImplementation(); //stage 1
//        private static Iassignment? s_dalAssign = new AssignmentImplementation(); //stage 1
//        private static Iconfig? s_dalConfig = new ConfigImplementation(); //stage 1

//        public static void Main(string[] args)
//        {
//            try
//            {
//                // קריאה לאתחול הנתונים
//                Initialization.Do(s_dalAssign, s_dalvolunteer, s_dalCall, s_dalConfig);

//                // הצגת התפריט הראשי
//                ShowMainMenu();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error: {ex.Message}");
//            }
//        }

//        private static void ShowMainMenu()
//        {
//            while (true)
//            {
//                Console.Clear();
//                Console.WriteLine("Main Menu:");
//                Console.WriteLine("1. Initialize Data");
//                Console.WriteLine("2. Show Volunteer Submenu");
//                Console.WriteLine("3. Show Call Submenu");
//                Console.WriteLine("4. Show Assignment Submenu");
//                Console.WriteLine("5. Show Config Submenu");
//                Console.WriteLine("6. Reset Data and Configuration");
//                Console.WriteLine("7. Exit");
//                Console.Write("Please choose an option: ");

//                string choice = Console.ReadLine();

//                switch (choice)
//                {
//                    case "1":
//                        InitializeData();
//                        break;
//                    case "2":
//                        ShowVolunteerSubmenu();
//                        break;
//                    case "3":
//                        ShowCallSubmenu();
//                        break;
//                    case "4":
//                        ShowAssignmentSubmenu();
//                        break;
//                    case "5":
//                        ShowConfigSubmenu();
//                        break;
//                    case "6":
//                        ResetDataAndConfiguration();
//                        break;
//                    case "7":
//                        return; // Exit program
//                    default:
//                        Console.WriteLine("Invalid choice, please try again.");
//                        break;
//                }
//            }
//        }


//            private static void InitializeData()
//            {
//                try
//                {
//                    // אתחול נתונים על ידי קריאה למתודה Initialization.Do
//                    Initialization.Do(s_dalAssign, s_dalvolunteer, s_dalCall, s_dalConfig);
//                    Console.WriteLine("Data Initialized Successfully.");
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error initializing data: {ex.Message}");
//                }
//            }

//            private static void ResetDataAndConfiguration()
//            {
//                try
//                {
//                    // איפוס של הנתונים על ידי קריאה למתודות למחיקת כל הרשימות
//                    s_dalvolunteer.DeleteAll();
//                    s_dalCall.DeleteAll();
//                    s_dalAssign.DeleteAll();

//                    // איפוס נתוני התצורה
//                    s_dalConfig.Reset();
//                    Console.WriteLine("Data and configuration reset successfully.");
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error resetting data: {ex.Message}");
//                }
//            }
//        }
//    private static void ShowVolunteerSubmenu()
//    {
//        while (true)
//        {
//            Console.Clear();
//            Console.WriteLine("Volunteer Submenu:");
//            Console.WriteLine("1. Add New Volunteer");
//            Console.WriteLine("2. View Volunteer by ID");
//            Console.WriteLine("3. View All Volunteers");
//            Console.WriteLine("4. Update Volunteer");
//            Console.WriteLine("5. Delete Volunteer");
//            Console.WriteLine("6. Delete All Volunteers");
//            Console.WriteLine("7. Back to Main Menu");
//            Console.Write("Please choose an option: ");

//            string choice = Console.ReadLine();

//            try
//            {
//                switch (choice)
//                {
//                    case "1":
//                        AddVolunteer();
//                        break;
//                    case "2":
//                        ViewVolunteerById();
//                        break;
//                    case "3":
//                        ViewAllVolunteers();
//                        break;
//                    case "4":
//                        UpdateVolunteer();
//                        break;
//                    case "5":
//                        DeleteVolunteer();
//                        break;
//                    case "6":
//                        DeleteAllVolunteers();
//                        break;
//                    case "7":
//                        return; // Back to Main Menu
//                    default:
//                        Console.WriteLine("Invalid choice, please try again.");
//                        break;
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error: {ex.Message}");
//            }
//        }

//    }
//    private static void AddVolunteer()
//    {
//        Console.WriteLine("Enter Volunteer details...");
//        // קלט נתונים מהמשתמש עבור המתנדב
//        // לדוגמה: שם, כתובת, טלפון, תאריך לידה
//        // נבצע יצירה והוספה דרך הממשק
//        var volunteer = new Volunteer();  // יצירת אובייקט חדש
//        s_dalvolunteer.createVolunteer(volunteer); // הוספה לרשימה
//        Console.WriteLine("Volunteer added successfully.");
//    }

//    private static void ViewVolunteerById()
//    {
//        Console.Write("Enter Volunteer ID: ");
//        int id = int.Parse(Console.ReadLine());
//        var volunteer = s_dalvolunteer.Read(id);
//        if (volunteer != null)
//        {
//            Console.WriteLine(volunteer);
//        }
//        else
//        {
//            Console.WriteLine("Volunteer not found.");
//        }
//    }

//    private static void ViewAllVolunteers()
//    {
//        var volunteers = s_dalvolunteer.ReadAll();
//        foreach (var volunteer in volunteers)
//        {
//            Console.WriteLine(volunteer);
//        }
//    }

//    private static void UpdateVolunteer()
//    {
//        // קלט מזהה של המתנדב ונתונים לעדכון
//        Console.Write("Enter Volunteer ID to update: ");
//        int id = int.Parse(Console.ReadLine());
//        var volunteer = s_dalvolunteer.Read(id);

//        if (volunteer != null)
//        {
//            Console.WriteLine("Current volunteer data: ");
//            Console.WriteLine(volunteer);
//            // קלוט נתונים חדשים לעדכון
//            Console.Write("Enter new name (leave empty to keep current): ");
//            string newName = Console.ReadLine();
//            if (!string.IsNullOrEmpty(newName))
//            {
//                volunteer.Name = newName; // עדכון נתון
//                s_dalvolunteer.Update(volunteer); // עדכון הממשק
//            }
//            Console.WriteLine("Volunteer updated successfully.");
//        }
//        else
//        {
//            Console.WriteLine("Volunteer not found.");
//        }
//    }

//    private static void DeleteVolunteer()
//    {
//        Console.Write("Enter Volunteer ID to delete: ");
//        int id = int.Parse(Console.ReadLine());
//        s_dalvolunteer.Delete(id); // מחיקת המתנדב
//        Console.WriteLine("Volunteer deleted successfully.");
//    }

//    private static void DeleteAllVolunteers()
//    {
//        s_dalvolunteer.DeleteAll(); // מחיקת כל המתנדבים
//        Console.WriteLine("All volunteers deleted successfully.");
//    }


//}

using Dal;
using DalApi;
using DO;

namespace DalTest
{
    internal class Program
    {
        // שדות סטטיים עבור הממשקים
        private static Ivolunteer? s_dalvolunteer = new VolunteerImplementation(); // stage 1
        private static Icall? s_dalCall = new CallImplementation(); // stage 1
        private static Iassignment? s_dalAssign = new AssignmentImplementation(); // stage 1
        private static Iconfig? s_dalConfig = new ConfigImplementation(); // stage 1

        public static void Main(string[] args)
        {
            try
            {
                // קריאה לאתחול הנתונים
                Initialization.Do(s_dalAssign, s_dalvolunteer, s_dalCall, s_dalConfig);

                // הצגת התפריט הראשי
                ShowMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // מתודת התפריט הראשי עם אפשרויות גישה לתתי תפריטים
        private static void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Main Menu:");
                Console.WriteLine("1. Initialize Data");
                Console.WriteLine("2. Show Volunteer Submenu");
                Console.WriteLine("3. Show Call Submenu");
                Console.WriteLine("4. Show Assignment Submenu");
                Console.WriteLine("5. Show Config Submenu");
                Console.WriteLine("6. Reset Data and Configuration");
                Console.WriteLine("7. Exit");
                Console.Write("Please choose an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        InitializeData();
                        break;
                    case "2":
                        ShowVolunteerSubmenu();
                        break;
                    case "3":
                        ShowCallSubmenu();
                        break;
                    case "4":
                        ShowAssignmentSubmenu();
                        break;
                    case "5":
                        ShowConfigSubmenu();
                        break;
                    case "6":
                        ResetDataAndConfiguration();
                        break;
                    case "7":
                        return; // יציאה מהתוכנית
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }

        // אתחול הנתונים
        private static void InitializeData()
        {
            try
            {
                Initialization.Do(s_dalAssign, s_dalvolunteer, s_dalCall, s_dalConfig);
                Console.WriteLine("Data Initialized Successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing data: {ex.Message}");
            }
        }

        // איפוס נתונים ותצורה
        private static void ResetDataAndConfiguration()
        {
            try
            {
                s_dalvolunteer.DeleteAll();
                s_dalCall.DeleteAll();
                s_dalAssign.DeleteAll();
                s_dalConfig.Reset(); // איפוס כל ערכי התצורה
                Console.WriteLine("Data and configuration reset successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting data: {ex.Message}");
            }
        }

        // תפריט משנה לפעולות מתנדבים
        private static void ShowVolunteerSubmenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Volunteer Submenu:");
                Console.WriteLine("1. Add New Volunteer");
                Console.WriteLine("2. View Volunteer by ID");
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
                            return; // חזרה לתפריט הראשי
                        default:
                            Console.WriteLine("Invalid choice, please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        // הוספת מתנדב חדש
        private static void AddVolunteer()
        {
            Console.WriteLine("Enter Volunteer details...");
            var volunteer = new Volunteer(); // יצירת אובייקט חדש של מתנדב
            s_dalvolunteer.createVolunteer(volunteer); // הוספת מתנדב לרשימה
            Console.WriteLine("Volunteer added successfully.");
        }

        // הצגת מתנדב לפי מזהה
        private static void ViewVolunteerById()
        {
            Console.Write("Enter Volunteer ID: ");
            int id = int.Parse(Console.ReadLine());
            var volunteer = s_dalvolunteer.Read(id);
            Console.WriteLine(volunteer != null ? volunteer.ToString() : "Volunteer not found.");
        }

        // הצגת כל המתנדבים
        private static void ViewAllVolunteers()
        {
            var volunteers = s_dalvolunteer.ReadAll();
            foreach (var volunteer in volunteers)
            {
                Console.WriteLine(volunteer);
            }
        }

        // עדכון מתנדב קיים
        private static void UpdateVolunteer()
        {
            Console.Write("Enter Volunteer ID to update: ");
            int id = int.Parse(Console.ReadLine());
            var volunteer = s_dalvolunteer.Read(id);

            if (volunteer != null)
            {
                Console.WriteLine("Current volunteer data: ");
                Console.WriteLine(volunteer);
                Console.Write("Enter new name (leave empty to keep current): ");
                string newName = Console.ReadLine();
                if (!string.IsNullOrEmpty(newName))
                {
                    volunteer.Name = newName;
                    s_dalvolunteer.Update(volunteer);
                }
                Console.WriteLine("Volunteer updated successfully.");
            }
            else
            {
                Console.WriteLine("Volunteer not found.");
            }
        }

        // מחיקת מתנדב לפי מזהה
        private static void DeleteVolunteer()
        {
            Console.Write("Enter Volunteer ID to delete: ");
            int id = int.Parse(Console.ReadLine());
            s_dalvolunteer.Delete(id);
            Console.WriteLine("Volunteer deleted successfully.");
        }

        // מחיקת כל המתנדבים
        private static void DeleteAllVolunteers()
        {
            s_dalvolunteer.DeleteAll();
            Console.WriteLine("All volunteers deleted successfully.");
        }

        // פונקציות משנה לתתי תפריטים של קריאות, שיוכים, ותצורה - יש להשלים בהתאם
        private static void ShowCallSubmenu() { /* מימוש  */ }
        private static void ShowAssignmentSubmenu() { /* מימוש  */ }
        private static void ShowConfigSubmenu() { /* מימוש  */ }
    }
}

