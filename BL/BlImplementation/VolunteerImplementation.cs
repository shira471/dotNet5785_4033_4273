namespace BlImplementation;
using System.Net;
using System.Text.Json;
using BlApi;
using BO;
using BO.Enums;
//using System;
using System.Net.Http;
using System.Threading.Tasks;
using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    // פונקציה פרטית להמרת כתובת לקואורדינטות
    //public static double[]? GetCoordinatesFromGoogle(string address)
    //{
    //    // Step 1: Validate the input
    //    if (string.IsNullOrWhiteSpace(address))
    //        throw new ArgumentException("Address cannot be null or empty.", nameof(address));

    //    // Step 2: Define the API key and base URL
    //    // TODO: Replace "YOUR_API_KEY" with your actual Google Maps API key.
    //    string apiKey = "AIzaSyDnyS5QMBa_4uwPOdbFH9T8_zNOXe3DzGw"; // Enter your Google Maps API key here.
    //    string apiUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

    //    // Step 3: Create an HttpClient to perform the request
    //    using (HttpClient client = new HttpClient())
    //    {
    //        try
    //        {
    //            // Step 4: Send the HTTP request synchronously
    //            HttpResponseMessage response = client.GetAsync(apiUrl).Result;

    //            // Step 5: Ensure the response is successful
    //            response.EnsureSuccessStatusCode();

    //            // Step 6: Read the response content synchronously
    //            string responseBody = response.Content.ReadAsStringAsync().Result;
    //            // Step 7: Parse the JSON response to extract latitude and longitude
    //            return ParseCoordinatesFromGoogle(responseBody);
    //        }
    //        catch (HttpRequestException)
    //        {
    //            // Handle cases like no internet or server unreachable
    //            throw new Exception("Unable to connect to the internet or server is unreachable.");
    //        }
    //        catch (Exception ex)
    //        {
    //            // Handle general exceptions
    //            throw new Exception("An error occurred while fetching coordinates.", ex);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Parses the JSON response from Google Maps API and extracts the latitude and longitude.
    ///// </summary>
    ///// <param name="responseBody">The JSON response from Google Maps API.</param>
    ///// <returns>
    ///// An array containing the latitude and longitude if found, null otherwise.
    ///// </returns>
    //private static double[]? ParseCoordinatesFromGoogle(string responseBody)
    //{
    //    // Parse the JSON response into a JObject
    //    JObject json = JObject.Parse(responseBody);

    //    // Check if the response contains a valid result
    //    JToken results = json["results"];
    //    if (results != null && results.HasValues)
    //    {
    //        JToken location = results[0]["geometry"]["location"];
    //        return new double[]
    //        {
    //        (double)location["lat"],
    //        (double)location["lng"]
    //        };
    //    }

    //    // If no results were found, return null
    //    return null;
    //}
}
