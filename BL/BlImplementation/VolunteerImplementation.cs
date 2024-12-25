namespace BlImplementation;
using System.Net;
using System.Text.Json;
using BlApi;
using BO;
//using System;
using System.Net.Http;
using System.Threading.Tasks;
using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BL.Helpers;

//using Newtonsoft.Json.Linq;
public class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.Idal _dal = DalApi.Factory.Get;

    public void AddVolunteer(Volunteer volunteer)
    {
        if (volunteer == null)
        {
            throw new ArgumentNullException(nameof(volunteer), "Volunteer cannot be null");
        }

        try
        {
            var temp = VolunteerManager.GetCoordinatesFromGoogle(volunteer.Address);
            // המרה מ-BO.Volunteer ל-DO.Volunteer
            var dalVolunteer = new DO.Volunteer
            {
                idVol = volunteer.Id,                  // מזהה המתנדב
                adress = volunteer.Address,             // כתובת המתנדב
                name = volunteer.FullName,              // שם המתנדב
                email = volunteer.Email,                // דוא"ל המתנדב
                phoneNumber = volunteer.Phone,          // מספר טלפון המתנדב
                password = volunteer.Password ?? "",    // סיסמת המתנדב
                latitude = temp[0] ,     // קו רוחב
                longitude = temp[1],   // קו אורך
                limitDestenation = volunteer.MaxDistance ?? 0, // מגבלת המרחק
                isActive = volunteer.IsActive,          // האם פעיל
           //     role = DO.Role.Volunteer,
                distanceType = (DO.Hamal)volunteer.DistanceType // המרה לשדה מתאים
            };

            // קריאה לפונקציה ב-DAL להוספת המתנדב
            _dal.volunteer.Create(dalVolunteer);

            Console.WriteLine("Volunteer added successfully");
            VolunteerManager.Observers.NotifyListUpdated(); //stage 5
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות במידה ויש
            Console.WriteLine($"Error while adding volunteer: {ex.Message}");
            throw;
        }
    }




    public void DeleteVolunteer(int volunteerId)
    {
        try
        {
            // קריאה ל-DAL למחיקת המתנדב לפי מזהה
            _dal.volunteer.Delete(volunteerId);

            // אם הצלחנו, הודעה שתצא למסך
            Console.WriteLine($"Volunteer with ID={volunteerId} deleted successfully");
        }
        catch (Exception ex)
        {
            // טיפול בשגיאה במידה ולא הצלחנו למחוק את המתנדב
            Console.WriteLine($"Error while deleting volunteer: {ex.Message}");
            throw;
        }
        VolunteerManager.Observers.NotifyListUpdated(); //stage 5

    }


    public Volunteer GetVolunteerDetails(int volunteerId)
    {
        try
        {
            // קריאה ל-DAL לקבלת המתנדב לפי מזהה
            var volunteerDO = _dal.volunteer.Read(volunteerId);

            if (volunteerDO == null)
            {
                // אם המתנדב לא נמצא, נזרוק חריגה מתאימה
                throw new Exception($"Volunteer with ID={volunteerId} does not exist.");
            }
            var dalVolunteer = new Volunteer(
    volunteerId,
    volunteerDO.name,
    volunteerDO.phoneNumber,
    volunteerDO.email,
    volunteerDO.adress,
    volunteerDO.isActive
//true // Assuming the volunteer is active, set this as needed
);
            // dalVolunteer.IsActive = volunteerDO.isActive;
            dalVolunteer.Address = volunteerDO.adress;
            dalVolunteer.Password = volunteerDO.password;
            dalVolunteer.Latitude = volunteerDO.latitude;
            dalVolunteer.Longitude = volunteerDO.longitude;
            dalVolunteer.Phone = volunteerDO.phoneNumber;
            //dalVolunteer.Role = volunteerDO.role;
            return dalVolunteer;
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות במקרה של בעיה
            Console.WriteLine($"Error while getting volunteer details: {ex.Message}");
            throw;
        }
    }
    public IEnumerable<VolunteerInList> GetVolunteersList(bool? isActive = null, VolunteerSortBy? sortBy = null)
    {
            try
            {
                // שליפת כל המתנדבים מתוך מקור הנתונים
                var volunteers = _dal.volunteer.ReadAll();

                // סינון לפי ערך isActive אם הוזן
                if (isActive.HasValue)
                {
                    volunteers = volunteers.Where(v => v.isActive == isActive.Value);
                }

                // מיון לפי ערך sortBy אם הוזן
                if (sortBy.HasValue)
                {
                    switch (sortBy.Value)
                    {
                        case VolunteerSortBy.Id:
                            volunteers = volunteers.OrderBy(v => v.idVol);
                            break;
                        case VolunteerSortBy.Name:
                            volunteers = volunteers.OrderBy(v => v.name);
                            break;
                        case VolunteerSortBy.ActivityStatus:
                            volunteers = volunteers.OrderBy(v => v.isActive);
                            break;
                        default:
                            volunteers = volunteers.OrderBy(v => v.idVol); // ברירת מחדל
                            break;
                    }
                }
                else
                {
                    // מיון לפי ת.ז אם sortBy == null
                    volunteers = volunteers.OrderBy(v => v.idVol);
                }

            // המרה מ-DO.Volunteer ל-BO.VolunteerInList
            return volunteers.Select(v => new VolunteerInList(
                v.idVol,   // id
                v.name,    // FullName
                v.isActive // IsActive

            ));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting volunteers list: {ex.Message}");
                throw;
            }
    }



    public string Login(string username, string password)
    {
        try
        {
            // שליפת כל המתנדבים מ-DAL
            var volunteers = _dal.volunteer.ReadAll();

            // חיפוש מתנדב לפי שם משתמש וסיסמה
            var volunteer = volunteers.FirstOrDefault(v => v.email == username && v.password == password);

            if (volunteer == null)
            {
                // אם לא נמצא מתנדב עם שם משתמש וסיסמה תואמים, נזרוק חריגה
                throw new UnauthorizedAccessException("שם משתמש או סיסמה שגויים.");
            }

            // החזרת תפקיד המשתמש

            return volunteer.role switch
            {
                DO.Role.Manager => "Manager",     // תפקיד: מנהל
                DO.Role.Volunteer => "Volunteer", // תפקיד: מתנדב
                _ => throw new InvalidOperationException("Invalid role")
            };


        }
        catch (Exception ex)
        {
            // טיפול בשגיאות
            Console.WriteLine($"Error during login: {ex.Message}");
            throw;
        }
    }





    public void UpdateVolunteer(int requesterId, BO.Volunteer volunteer)
    {
        // בדיקה ראשונית האם האובייקט שהתקבל הוא null
        if (volunteer == null)
            throw new ArgumentNullException(nameof(volunteer), "Volunteer object cannot be null");

        // בדיקת הרשאות: רק מנהל או המתנדב עצמו יכולים לעדכן את הפרטים
        if (requesterId != volunteer.Id && !_dal.volunteer.Read(requesterId).role.Equals(DO.Role.Manager))
            throw new UnauthorizedAccessException("Only managers or the volunteer themselves can update details.");

        // בדיקת פורמט תקינות בסיסי (לדוגמה: פורמט אימייל תקין)
        if (!IsValidEmail(volunteer.Email))
            throw new ArgumentException("Invalid email format");

        // בדיקת תקינות מספר טלפון (שדה מספרי)
        if (!IsValidPhoneNumber(volunteer.Phone))
            throw new ArgumentException("Invalid phone number");

        // בדיקת תקינות תעודת זהות (מספרי עם ספרת ביקורת תקינה)
        if (!IsValidId(volunteer.Id))
            throw new ArgumentException("Invalid ID number");

        // בקשת הרשומה המקורית משכבת הנתונים
        var existingVolunteer = _dal.volunteer.Read(volunteer.Id)
            ?? throw new KeyNotFoundException($"Volunteer with ID {volunteer.Id} not found");

        // בדיקת שדות שניתן לעדכן רק על ידי מנהל
        if (!object.Equals(existingVolunteer.role, volunteer.Role) &&
            !object.Equals(_dal.volunteer.Read(requesterId).role, DO.Role.Manager))
        {
            throw new UnauthorizedAccessException("Only a manager can update the volunteer's role.");
        }

        // עדכון שדות קווי אורך ורוחב על פי כתובת חדשה (אם הכתובת השתנתה)
        if (existingVolunteer.adress != volunteer.Address)
        {
            var coordinates = VolunteerManager.GetCoordinatesFromGoogle(volunteer.Address);
            if (coordinates == null)
                throw new ArgumentException("Invalid address provided");
            //volunteer.Latitude = coordinates.Value.Latitude;
            //volunteer.Longitude = coordinates.Value.Longitude;
        }

        // המרת אובייקט BO.Volunteer ל-DO.Volunteer
        var updatedVolunteer = new DO.Volunteer(
            idVol: volunteer.Id,
            adress: volunteer.Address,
            name: volunteer.FullName,
            email: volunteer.Email,
            phoneNumber: volunteer.Phone,
            password: volunteer.Password ?? existingVolunteer.password,
            latitude: volunteer.Latitude ?? existingVolunteer.latitude,
            longitude: volunteer.Longitude ?? existingVolunteer.longitude,
            limitDestenation: volunteer.MaxDistance ?? existingVolunteer.limitDestenation,
            isActive: volunteer.IsActive,
            role: (DO.Role?)volunteer.Role,
            distanceType: (DO.Hamal?)volunteer.DistanceType
        );

        // ניסיון לעדכן את המתנדב בשכבת הנתונים
        try
        {
            _dal.volunteer.Update(updatedVolunteer);
        }
        catch (Exception ex)
        {
            // תפיסת חריגה משכבת הנתונים והשלכת חריגה מתאימה לשכבת התצוגה
            throw new Exception("Failed to update the volunteer details.", ex);
        }
        VolunteerManager.Observers.NotifyItemUpdated(updatedVolunteer.idVol); //stage 5
        VolunteerManager.Observers.NotifyListUpdated(); //stage 5

    }

    // פונקציה פרטית לבדיקה האם אימייל תקין
    private bool IsValidEmail(string email)
    {
        try
        {
            var mail = new System.Net.Mail.MailAddress(email);
            return mail.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // פונקציה פרטית לבדיקה האם מספר טלפון תקין
    private bool IsValidPhoneNumber(string phoneNumber)
    {
        return phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 7;
    }

    // פונקציה פרטית לבדיקה האם תעודת זהות תקינה
    private bool IsValidId(int id)
    {
        // לוגיקת בדיקת ספרת ביקורת
        return id > 0 && id.ToString().Length == 9; // לדוגמה בלבד
    }

    public void AddObserver(Action listObserver)=>
        VolunteerManager.Observers.AddListObserver(listObserver); //stage 5

    public void AddObserver(int id, Action observer) =>
        VolunteerManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
        VolunteerManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
        VolunteerManager.Observers.RemoveObserver(id, observer); //stage 5

}
