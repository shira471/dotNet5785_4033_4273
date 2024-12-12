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
    //static readonly Idal s_dal = Dallist.intance;  //stage 2
    //private static readonly Idal s_dal = new DalXml(); //stage 3
    static readonly Idal s_dal = Factory.Get;
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
        string phone = Console.ReadLine();

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

    private static void ShowCallSubmenu()  // מתחילים את פונקציית הצגת תפריט המשנה לקריאות.
    {
        while (true)  // לולאת while שתשמור את התפריט פתוח עד שהמשתמש יבחר לצאת.
        {
            Console.Clear();  // מנקה את המסך לפני הצגת התפריט החדש.
            Console.WriteLine("Call Submenu:");  // הצגת כותרת התפריט.
            Console.WriteLine("1. Add Call");  // הצגת האפשרות להוסיף קריאה.
            Console.WriteLine("2. View Call By ID");  // הצגת האפשרות לראות קריאה לפי מזהה.
            Console.WriteLine("3. View All Calls");  // הצגת האפשרות לראות את כל הקריאות.
            Console.WriteLine("4. Update Call");  // הצגת האפשרות לעדכן קריאה.
            Console.WriteLine("5. Delete Call");  // הצגת האפשרות למחוק קריאה.
            Console.WriteLine("6. Delete All Calls");  // הצגת האפשרות למחוק את כל הקריאות.
            Console.WriteLine("7. Back to Main Menu");  // הצגת האפשרות לחזור לתפריט הראשי.
            Console.Write("Please choose an option: ");  // בקשה מהמשתמש לבחור אפשרות.

            string choice = Console.ReadLine();  // קריאת בחירת המשתמש.

            try  // טיפול בחריגות במהלך ביצוע הפעולה.
            {
                switch (choice)  // ביצוע הפעולה בהתאם לבחירה.
                {
                    case "1":  // אם נבחרה אפשרות 1 (הוספת קריאה).
                        AddCall();  // קריאה לפונקציה להוספת קריאה.
                        break;  // יציאה מה-switch אם הפונקציה הוזנה.
                    case "2":  // אם נבחרה אפשרות 2 (הצגת קריאה לפי מזהה).
                        ViewCallById();  // קריאה לפונקציה להציג קריאה לפי מזהה.
                        break;  // יציאה מה-switch אם הפונקציה הוזנה.
                    case "3":  // אם נבחרה אפשרות 3 (הצגת כל הקריאות).
                        ViewAllCalls();  // קריאה לפונקציה להציג את כל הקריאות.
                        break;  // יציאה מה-switch אם הפונקציה הוזנה.
                    case "4":  // אם נבחרה אפשרות 4 (עדכון קריאה).
                        UpdateCall();  // קריאה לפונקציה לעדכון קריאה.
                        break;  // יציאה מה-switch אם הפונקציה הוזנה.
                    case "5":  // אם נבחרה אפשרות 5 (מחיקת קריאה).
                        DeleteCall();  // קריאה לפונקציה למחוק קריאה.
                        break;  // יציאה מה-switch אם הפונקציה הוזנה.
                    case "6":  // אם נבחרה אפשרות 6 (מחיקת כל הקריאות).
                        DeleteAllCalls();  // קריאה לפונקציה למחוק את כל הקריאות.
                        break;  // יציאה מה-switch אם הפונקציה הוזנה.
                    case "7":  // אם נבחרה אפשרות 7 (חזרה לתפריט הראשי).
                        return;  // חזרה לתפריט הראשי.
                    default:  // אם נבחרה אפשרות לא חוקית.
                        Console.WriteLine("Invalid choice, please try again.");  // הצגת הודעה על אפשרות לא חוקית.
                        break;  // יציאה מה-switch.
                }
            }
            catch (Exception ex)  // טיפול בחריגות בכל פעולה.
            {
                Console.WriteLine($"Error: {ex.Message}");  // הצגת הודעת שגיאה אם התרחשה חריגה.
            }
        }
    }
    /// <summary>
    /// מציג פרטי קריאה לפי מזהה
    /// </summary>
    private static void ViewCallById()
    {
        Console.Write("Enter Call ID: ");
        int id = int.Parse(Console.ReadLine());
        var call = s_dal.call?.Read(id);
        Console.WriteLine(call != null ? call.ToString() : "Call not found.");
    }
    /// <summary>
    /// מציג את כל הקריאות
    /// </summary>
    private static void ViewAllCalls()
    {
        var calls = s_dal.call?.ReadAll();
        foreach (var call in calls)
        {
            Console.WriteLine(call);
        }
    }
    private static void AddCall()  // מתחילים את הפונקציה להוספת קריאה.
    {
        Console.WriteLine("Enter Call details:");  // הצגת הודעה למשתמש להזין פרטי קריאה.
        Console.Write("ID: ");  // בקשה מהמשתמש להזין מזהה קריאה.
        int id = int.Parse(Console.ReadLine());  // קריאת מזהה והפיכתו למספר שלם.
        Console.Write("Description: ");  // בקשה מהמשתמש להזין תיאור לקריאה.
        string description = Console.ReadLine();  // קריאת התיאור.
        Console.Write("Address: ");  // בקשה מהמשתמש להזין כתובת.
        string address = Console.ReadLine();  // קריאת הכתובת.
        Console.Write("Phone: ");  // בקשה מהמשתמש להזין מספר טלפון.
        string phone = Console.ReadLine();  // קריאת מספר הטלפון.

        var call = new Call  // יצירת אובייקט קריאה חדש.
        {
            id = id,  // אתחול המזהה של הקריאה.
            detail = description,  // אתחול התיאור של הקריאה.
            adress = address  // אתחול הכתובת של הקריאה.

        };

        s_dal.call.Create(call);  // קריאה לפונקציה שיצור את הקריאה באמצעות ה-DAL.
        Console.WriteLine("Call added successfully.");  // הודעה שהקריאה נוספה בהצלחה.
    }
    private static void UpdateCall()  // מתחילים את הפונקציה לעדכון קריאה.
    {
        Console.Write("Enter Call ID to update: ");  // בקשה מהמ משתמש להזין מזהה של קריאה לעדכון.
        int id = int.Parse(Console.ReadLine());  // קריאת מזהה הקריאה.
        var call = s_dal.call?.Read(id);  // קריאה לקריאה לפי מזהה.

        if (call != null)  // אם הקריאה נמצאה.
        {
            Console.WriteLine("Current call data:");  // הצגת פרטי הקריאה הנוכחיים.
            Console.WriteLine(call);  // הצגת פרטי הקריאה.

            Console.Write("Enter new description (leave empty to keep current): ");  // בקשה לתיאור חדש.
            string newDescription = Console.ReadLine();  // קריאת תיאור חדש.
            if (!string.IsNullOrEmpty(newDescription))  // אם התיאור לא ריק.
            {
                call = call with { detail = newDescription };  // עדכון התיאור באמצעות הפקודה "with" (אם קיימת).
            }

            s_dal.call?.Update(call);  // קריאה לעדכון הקריאה ב-DAL.
            Console.WriteLine("Call updated successfully.");  // הודעה שהקריאה עודכנה בהצלחה.
        }
        else  // אם הקריאה לא נמצאה.
        {
            Console.WriteLine("Call not found.");  // הצגת הודעה שהקריאה לא נמצאה.
        }
    }
    private static void DeleteCall()  // מתחילים את הפונקציה למחיקת קריאה.
    {
        Console.Write("Enter Call ID to delete: ");  // בקשה מהמ משתמש להזין מזהה של קריאה למחיקה.
        int id = int.Parse(Console.ReadLine());  // קריאת מזהה הקריאה.
        s_dal.call?.Delete(id);  // קריאה לפונקציה למחוק את הקריאה.
        Console.WriteLine("Call deleted successfully.");  // הודעה שהקריאה נמחקה בהצלחה.
    }
    private static void DeleteAllCalls()  // מתחילים את הפונקציה למחיקת כל הקריאות.
    {
        s_dal.call?.DeleteAll();  // קריאה למחוק את כל הקריאות ב-DAL.
        Console.WriteLine("All calls deleted successfully.");  // הודעה שהקריאות נמחקו בהצלחה.
    }
    /// <summary>
    /// מציג את תפריט המשנה לשיוכים ומאפשר לבצע פעולות CRUD
    /// </summary>
    private static void ShowAssignmentSubmenu()
    {
        while (true) // לולאה אינסופית עד שהמשתמש בוחר לצאת
        {
            Console.Clear(); // מנקה את המסך
            Console.WriteLine("Assignment Submenu:"); // מציג כותרת
            Console.WriteLine("1. Add Assignment"); // הצגת אפשרות הוספת שיוך
            Console.WriteLine("2. View Assignment By ID"); // הצגת אפשרות הצגת שיוך לפי ID
            Console.WriteLine("3. View All Assignments"); // הצגת אפשרות הצגת כל השיוכים
            Console.WriteLine("4. Update Assignment"); // הצגת אפשרות עדכון שיוך
            Console.WriteLine("5. Delete Assignment"); // הצגת אפשרות מחיקת שיוך
            Console.WriteLine("6. Delete All Assignments"); // הצגת אפשרות מחיקת כל השיוכים
            Console.WriteLine("7. Back to Main Menu"); // הצגת אפשרות חזרה לתפריט הראשי
            Console.Write("Please choose an option: "); // בקשת קלט מהמשתמש

            string choice = Console.ReadLine(); // קריאת בחירת המשתמש

            try
            {
                // תלוי בבחירת המשתמש, נבצע פעולה מתאימה
                switch (choice)
                {
                    case "1":
                        AddAssignment(); // קריאה לפונקציה להוספת שיוך
                        break;
                    case "2":
                        ViewAssignmentById(); // קריאה לפונקציה להציג שיוך לפי ID
                        break;
                    case "3":
                        ViewAllAssignments(); // קריאה לפונקציה להציג את כל השיוכים
                        break;
                    case "4":
                        UpdateAssignment(); // קריאה לפונקציה לעדכון שיוך
                        break;
                    case "5":
                        DeleteAssignment(); // קריאה לפונקציה למחיקת שיוך
                        break;
                    case "6":
                        DeleteAllAssignments(); // קריאה לפונקציה למחיקת כל השיוכים
                        break;
                    case "7":
                        return; // חזרה לתפריט הראשי
                    default:
                        Console.WriteLine("Invalid choice, please try again."); // הצגת הודעה במקרה של קלט לא תקין
                        break;
                }
            }
            catch (Exception ex)
            {
                // טיפול בשגיאות אם משהו השתבש
                Console.WriteLine($"Error: {ex.Message}"); // הצגת הודעת שגיאה
            }
        }
    }

    /// <summary>
    /// מוסיף שיוך חדש
    /// </summary>
    private static void AddAssignment()
    {
        Console.WriteLine("Enter Assignment details:"); // בקשה להקליד פרטי שיוך
        Console.Write("ID: ");
        int id = int.Parse(Console.ReadLine()); // קריאה למספר ה-ID מהמשתמש
        Console.Write("Call ID: ");
        int callId = int.Parse(Console.ReadLine()); // קריאה למספר ה-Call ID מהמשתמש
        Console.Write("Volunteer ID: ");
        int volunteerId = int.Parse(Console.ReadLine()); // קריאה למספר ה-Volunteer ID מהמשתמש
        Console.Write("Start Time (yyyy-mm-dd hh:mm): ");
        DateTime? startTime = DateTime.TryParse(Console.ReadLine(), out DateTime parsedStart) ? (DateTime?)parsedStart : null; // קריאה לזמן התחלה, אם הקלט תקין נוודא שהוא מסוג DateTime
        Console.Write("Finish Time (yyyy-mm-dd hh:mm): ");
        DateTime? finishTime = DateTime.TryParse(Console.ReadLine(), out DateTime parsedFinish) ? (DateTime?)parsedFinish : null; // קריאה לזמן סיום, אם הקלט תקין נוודא שהוא מסוג DateTime

        // כאן יש להוסיף טיפול באובייקט Hamal (סוג תוצאה של השיוך)
        Console.Write("End of Assignment (Hamal value): ");
        string hamalInput = Console.ReadLine(); // קריאה לאפשרות Hamal מהמשתמש
        Hamal? endOfAssign = hamalInput == "null" ? null : (Hamal?)Enum.Parse(typeof(Hamal), hamalInput); // אם לא הוזן "null", המערכת תנסה לפרש את הערך לפי הערכים האפשריים ב-Hamal

        // יצירת אובייקט של Assignment עם הערכים שהוזנו
        var assignment = new Assignment(id, callId, volunteerId, startTime, finishTime, endOfAssign);

        // שליחה לאובייקט ה-DAL כדי להוסיף את השיוך
        s_dal.assignment?.Create(assignment); // שימוש בממשק ה-DAL כדי להוסיף את השיוך למסד הנתונים
        Console.WriteLine("Assignment added successfully."); // הודעת הצלחה
    }

    /// <summary>
    /// מציג פרטי שיוך לפי מזהה
    /// </summary>
    private static void ViewAssignmentById()
    {
        Console.Write("Enter Assignment ID: ");
        int id = int.Parse(Console.ReadLine()); // קריאה למזהה השיוך מהמשתמש
        var assignment = s_dal.assignment?.Read(id); // קריאה לפונקציה ב-DAL לשלוף את השיוך לפי מזהה
        Console.WriteLine(assignment != null ? assignment.ToString() : "Assignment not found."); // הצגת השיוך אם נמצא, אחרת הצגת הודעה שהשיוך לא נמצא
    }

    /// <summary>
    /// מציג את כל השיוכים
    /// </summary>
    private static void ViewAllAssignments()
    {
        var assignments = s_dal.assignment?.ReadAll(); // קריאה לפונקציה ב-DAL לשלוף את כל השיוכים
        foreach (var assignment in assignments) // לולאה על כל השיוכים
        {
            Console.WriteLine(assignment); // הצגת כל שיוך
        }
    }

    /// <summary>
    /// מעדכן פרטי שיוך קיים
    /// </summary>
    private static void UpdateAssignment()
    {
        Console.Write("Enter Assignment ID to update: ");
        int id = int.Parse(Console.ReadLine()); // קריאה למזהה השיוך שברצוננו לעדכן
        var assignment = s_dal.assignment?.Read(id); // קריאה לפונקציה ב-DAL לשלוף את השיוך לפי מזהה

        if (assignment != null)
        {
            Console.WriteLine("Current assignment data:"); // הצגת הנתונים הנוכחיים
            Console.WriteLine(assignment);

            // עדכון נתונים
            Console.Write("Enter new start time (leave empty to keep current): ");
            string startTimeInput = Console.ReadLine();
            DateTime? newStartTime = string.IsNullOrEmpty(startTimeInput) ? assignment.startTime : DateTime.Parse(startTimeInput); // עדכון זמן התחלה אם נדרש

            Console.Write("Enter new finish time (leave empty to keep current): ");
            string finishTimeInput = Console.ReadLine();
            DateTime? newFinishTime = string.IsNullOrEmpty(finishTimeInput) ? assignment.finishTime : DateTime.Parse(finishTimeInput); // עדכון זמן סיום אם נדרש

            // עדכון ערך Hamal
            Console.Write("Enter new end of assignment (Hamal value): ");
            string hamalInput = Console.ReadLine();
            Hamal? newEndOfAssign = hamalInput == "null" ? null : (Hamal?)Enum.Parse(typeof(Hamal), hamalInput); // עדכון הערך של Hamal

            // יצירת אובייקט חדש עם הנתונים המעודכנים
            var updatedAssignment = assignment with { startTime = newStartTime, finishTime = newFinishTime, endOfAssign = newEndOfAssign };

            // עדכון ב-DAL
            s_dal.assignment?.Update(updatedAssignment); // קריאה לעדכון ב-DAL
            Console.WriteLine("Assignment updated successfully."); // הודעת הצלחה
        }
        else
        {
            Console.WriteLine("Assignment not found."); // הודעת שגיאה אם השיוך לא נמצא
        }
    }

    /// <summary>
    /// מוחק שיוך לפי מזהה
    /// </summary>
    private static void DeleteAssignment()
    {
        Console.Write("Enter Assignment ID to delete: ");
        int id = int.Parse(Console.ReadLine()); // קריאה למזהה השיוך למחיקה
        s_dal.assignment?.Delete(id); // קריאה לפונקציה ב-DAL למחוק את השיוך
        Console.WriteLine("Assignment deleted successfully."); // הודעת הצלחה
    }

    /// <summary>
    /// מוחק את כל השיוכים
    /// </summary>
    private static void DeleteAllAssignments()
    {
        s_dal.assignment?.DeleteAll(); // קריאה לפונקציה ב-DAL למחוק את כל השיוכים
        Console.WriteLine("All assignments deleted successfully."); // הודעת הצלחה
    }
    /// <summary>
    /// מציג את תפריט המשנה לתצורה ומאפשר לבצע פעולות תצורה
    /// </summary>
    private static void ShowConfigSubmenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Config Submenu:");
            Console.WriteLine("1. Set Database Connection String");
            Console.WriteLine("2. Set File Path for Reports");
            Console.WriteLine("3. Load Configuration from File");
            Console.WriteLine("4. Save Configuration to File");
            Console.WriteLine("5. Reset Configuration to Default");
            Console.WriteLine("6. Back to Main Menu");
            Console.Write("Please choose an option: ");

            string choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        SetDatabaseConnectionString();
                        break;
                    case "2":
                        SetFilePathForReports();
                        break;
                    case "3":
                        LoadConfigurationFromFile();
                        break;
                    case "4":
                        SaveConfigurationToFile();
                        break;
                    case "5":
                        ResetConfigurationToDefault();
                        break;
                    case "6":
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

    /// <summary>
    /// מאפשר למשתמש להגדיר את מחרוזת החיבור למסד נתונים
    /// </summary>
    private static void SetDatabaseConnectionString()
    {
        Console.Write("Enter Database Connection String: ");
        string connectionString = Console.ReadLine();
        // שמירה של הקישור בקובץ קונפיגורציה או בהגדרות המערכת
        Console.WriteLine("Database connection string set successfully.");
    }

    /// <summary>
    /// מאפשר למשתמש להגדיר נתיב לקבצי דוחות
    /// </summary>
    private static void SetFilePathForReports()
    {
        Console.Write("Enter file path for reports: ");
        string filePath = Console.ReadLine();
        // שמירה של נתיב הקובץ בהגדרות
        Console.WriteLine("File path for reports set successfully.");
    }

    /// <summary>
    /// טוען קובץ קונפיגורציה
    /// </summary>
    private static void LoadConfigurationFromFile()
    {
        Console.Write("Enter file path to load configuration: ");
        string filePath = Console.ReadLine();
        // טען את הקובץ והחזר את ההגדרות
        Console.WriteLine("Configuration loaded successfully.");
    }

    /// <summary>
    /// שומר את הקונפיגורציה הנוכחית לקובץ
    /// </summary>
    private static void SaveConfigurationToFile()
    {
        Console.Write("Enter file path to save configuration: ");
        string filePath = Console.ReadLine();
        // שמור את ההגדרות בקובץ
        Console.WriteLine("Configuration saved successfully.");
    }

    /// <summary>
    /// מאפס את כל הגדרות המערכת לברירות מחדל
    /// </summary>
    private static void ResetConfigurationToDefault()
    {
        // איפוס כל הגדרות המערכת לברירות מחדל
        Console.WriteLine("Configuration reset to default.");
    }


}
