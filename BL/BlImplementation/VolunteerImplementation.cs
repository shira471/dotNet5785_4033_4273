namespace BlImplementation;

using System.Collections.Generic;
using System.Net;
using BlApi;
using BO;
using BO.Enums;

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
            // המרה מ-BO.Volunteer ל-DO.Volunteer
            var dalVolunteer = new DO.Volunteer
            {
                idVol = volunteer.Id,                  // מזהה המתנדב
                adress = volunteer.Address,             // כתובת המתנדב
                name = volunteer.FullName,              // שם המתנדב
                email = volunteer.Email,                // דוא"ל המתנדב
                phoneNumber = volunteer.Phone,          // מספר טלפון המתנדב
                password = volunteer.Password ?? "",    // סיסמת המתנדב
                latitude = volunteer.Latitude ?? 0,     // קו רוחב
                longitude = volunteer.Longitude ?? 0,   // קו אורך
                limitDestenation = volunteer.MaxDistance ?? 0, // מגבלת המרחק
                isActive = volunteer.IsActive,          // האם פעיל
                //role = volunteer.Role == BO.Role.Manager ? DO.Hamal.Role : DO.Hamal.Breakfast, // תפקיד המתנדב (לדוגמה)
                distanceType = (DO.Hamal)volunteer.DistanceType // המרה לשדה מתאים
            };

            // קריאה לפונקציה ב-DAL להוספת המתנדב
            _dal.volunteer.Create(dalVolunteer);

            Console.WriteLine("Volunteer added successfully");
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
    true // Assuming the volunteer is active, set this as needed
);
            dalVolunteer.Address = volunteerDO.adress;
            dalVolunteer.Password = volunteerDO.password;
            dalVolunteer.Latitude = volunteerDO.latitude;
            dalVolunteer.Longitude = volunteerDO.longitude;
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
        //try
        //{
        //    // משערים שיש לך דרך לשלוף את כל המתנדבים מתוך DataSource
        //    var volunteers = _dal.volunteer.ReadAll(); // אם המתודה קיימת ב-DAL
        //                                               // סינון לפי ערך isActive אם הוזן
        //    if (isActive.HasValue)
        //    {
        //        volunteers = volunteers.Where(v => v.isActive == isActive.Value);
        //    }
        //    // מיון לפי ערך sortBy אם הוזן
        //    if (sortBy.HasValue)
        //    {
        //        switch (sortBy.Value)
        //        {
        //            case VolunteerSortBy.Id:
        //                volunteers = volunteers.OrderBy(v => v.idVol);
        //                break;
        //            case VolunteerSortBy.Name:
        //                volunteers = volunteers.OrderBy(v => v.name);
        //                break;
        //            case VolunteerSortBy.ActivityStatus:
        //                volunteers = volunteers.OrderBy(v => v.isActive);
        //                break;
        //            default:
        //                break; // במקרה שאין sortBy, לא נבצע מיון
        //        }
        //    }
        //    // המרה מ-DO.Volunteer ל-BO.VolunteerInList
        //    return volunteers.Select(v => new VolunteerInList(
        //        v.idVol,          // id
        //        v.name,           // FullName
        //        v.isActive       // IsActive
        //    ));
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"Error while getting volunteers list: {ex.Message}");
        //    throw;
        //}
        throw new NotImplementedException();

    }


    public string Login(string username, string password)
    {
        throw new NotImplementedException();
    }

    public void UpdateVolunteer(int requesterId, Volunteer volunteer)
    {
        throw new NotImplementedException();
    }
}
