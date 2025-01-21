namespace DalTest;
using System.Buffers.Text;

using System;
using DalApi;
using DO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json.Linq;

public static class Initialization
{
    //private static Iassignment? s_dalAssignment; //stage 1
    //private static Icall? s_dalCall; //stage 1
    //private static Ivolunteer? s_dalvolunteer; //stage 1
    //private static Iconfig? s_dalconfig; //stage 1
    private static Idal? s_dal;//stage 2                                   // Random instance for generating values
    private static readonly Random s_rand = new Random();
    // רשימות לשמירת מזהי קריאות ומתנדבים
    private static List<int> callIds = new List<int>();
    private static List<int> volunteerIds = new List<int>();
   

    public static void creatVolunteer()
    {
        // Random instance
        Random s_rand = new Random();

        // Predefined lists
        string[] names = { "Avigail barmi","Namma Koufman","Hodaya Carcom","Soll Atall","Rivka Azrad",
                 "Sara Alvesson","Aviad Dan","Ori Dayan","Shimon Choen","Elia Malachi","Liam Leon","Yagel Levi","Ari Eitan","Noa Levi","Noam Blau" };
        double[]? coordinates = GetCoordinatesFromGoogle("jerusalem");
        if (coordinates == null)
        {
            throw new Exception($"Could not fetch coordinates for address: jerusalem ");
        }
        double latitude = coordinates[0];
        double longitude = coordinates[1];
        //יצירת מנהל 1
        s_dal!.volunteer.Create(new Volunteer(212314033, "jerusalem", "shira alfasi", "syrhlpsy7@gmail.com", "0584084178", "58850090", latitude, longitude, 10, true, Role.Manager, TypeDistance.air));
        // הוספת מזהה המתנדב לרשימה
        volunteerIds.Add(212314033);
        s_dal!.volunteer.Create(new Volunteer(325004273, "jerusalem", "ahuvi winkler", "a@gmail.com", "0507678050", "1234567", latitude, longitude, 10, true, Role.Manager, TypeDistance.air));
        // הוספת מזהה המתנדב לרשימה
        volunteerIds.Add(325004273);
        string[] addresses = new string[]
         {
        "15 King David St, Jerusalem, Israel", "27 Ben Yehuda St, Jerusalem, Israel", "2 Jaffa St, Jerusalem, Israel", "10 HaPalmach St, Jerusalem, Israel",
        "3 Keren Hayesod St, Jerusalem, Israel","18 Shmuel HaNagid St, Jerusalem, Israel","5 Yaffo Rd, Jerusalem, Israel","6 Rabbi Akiva St, Jerusalem, Israel","34 Ein Karem, Jerusalem, Israel","12 HaNevi'im St, Jerusalem, Israel",
        "22 Mamilla Mall, Jerusalem, Israel","7 Zion Sq, Jerusalem, Israel","16 Herzl St, Jerusalem, Israel","40 Hillel St, Jerusalem, Israel","14 Agron St, Jerusalem, Israel","28 Emek Refaim St, Jerusalem, Israel"};

        // Loop through all names
        foreach (string name in names)
        {
            // Random ID
            int id = s_rand.Next(100000000, 999999999); // Assuming 7-digit IDs
                                                    // הוספת מזהה המתנדב לרשימה
            volunteerIds.Add(id);
            // מערך של 20 כתובות אמיתיות
         
            // Random address
            string address = addresses[s_rand.Next(addresses.Length)];

            // Random email
            string email = $"{name.Replace(" ", ".").ToLower()}@gmail.com";

            // Random phone number (10 digits)
            // Random Israeli phone number generator
            string[] prefixes = { "050", "052", "053", "054", "055", "058" }; // Common Israeli cellphone prefixes
            string prefix = prefixes[s_rand.Next(prefixes.Length)]; // Randomly select a prefix
            string phoneNumber = $"{prefix}{s_rand.Next(1000000, 10000000):D7}"; // Generate a 7-digit number and concatenate


            // Random password (alphanumeric, length between 6 and 10)
            string password = GenerateRandomPassword(6, 10);

            // Random limit of destination (between 5 and 50 kilometers)
            double limitDestination = Math.Round(s_rand.NextDouble() * 45 + 5, 2);

            // Random active status
            bool isActive = s_rand.Next(0, 2) == 1;

            
            // Call the BL function to get coordinates
            coordinates = GetCoordinatesFromGoogle(address);
            if (coordinates == null)
            {
                throw new Exception($"Could not fetch coordinates for address: {address}");
            }
             latitude = coordinates[0];
             longitude = coordinates[1];

            //// Random role and distance type (if the Hamal enum is defined)
            //Role? role = GetRandomRoleValue();
           
            // Create the volunteer with random values and save it in the database
            s_dal!.volunteer.Create(new Volunteer(id, address, name, email, phoneNumber, password, latitude, longitude, limitDestination, isActive, Role.Volunteer, TypeDistance.air));
        }
    }


    // Helper method to generate a random alphanumeric password
    private static string GenerateRandomPassword(int minLength, int maxLength)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        int length = s_rand.Next(minLength, maxLength + 1);
        return new string(Enumerable.Repeat(chars, length).Select(s => s[s_rand.Next(s.Length)]).ToArray());
    }

    // Helper method to get a random value of the Hamal enum, or null
    private static CallType? GetRandomHamalValue()
    {
        Array values = Enum.GetValues(typeof(CallType));
        return (CallType)values.GetValue(s_rand.Next(values.Length));
    }
    //private static Role? GetRandomRoleValue()
    //{
    //    Array values = Enum.GetValues(typeof(Role));
    //    return s_rand.Next(0, 2) == 1 ? (Role)values.GetValue(s_rand.Next(values.Length)) : (Role?)null;
    //}

    public static void creatCall()
{
    string[] details = {
        "Call for assistance", "Emergency call", "Routine check-in",
        "Follow-up call", "Service request", "Technical support",
        "Customer inquiry", "Appointment reminder", "Complaint report",
        "Survey feedback"
    };

    string[] addresses = {
        "15 King David St, Jerusalem, Israel",
        "27 Ben Yehuda St, Jerusalem, Israel",
        "52 Jaffa St, Jerusalem, Israel",
        "10 HaPalmach St, Jerusalem, Israel",
        "3 Keren Hayesod St, Jerusalem, Israel",
        "18 Shmuel HaNagid St, Jerusalem, Israel",
        "25 Yaffo Rd, Jerusalem, Israel",
        "6 Rabbi Akiva St, Jerusalem, Israel",
        "34 Ein Karem, Jerusalem, Israel",
        "12 HaNevi'im St, Jerusalem, Israel"
    };

    DateTime systemClock = s_dal.config.clock;
    DateTime start = new DateTime(systemClock.Year - 2, 1, 1);
    int range = (systemClock - start).Days;

    foreach (string detail in details)
    {
        //int id = Config.NextCallId; // שימוש במספר רץ
        string address = addresses[s_rand.Next(addresses.Length)];

        double[]? coordinates = GetCoordinatesFromGoogle(address);
        if (coordinates == null)
        {
            throw new Exception($"Could not fetch coordinates for address: {address}");
        }

        double latitude = coordinates[0];
        double longitude = coordinates[1];
        DateTime startTime = start.AddDays(s_rand.Next(range));
        DateTime maximumTime = startTime.AddMinutes(s_rand.Next(1, 60));

        if (startTime.Year > 2024) startTime = new DateTime(2024, 12, 31, 23, 59, 59);
        if (maximumTime.Year > 2024) maximumTime = new DateTime(2024, 12, 31, 23, 59, 59);

        s_dal.call.Create(new Call(0, detail, address, latitude, longitude, GetRandomHamalValue(), startTime, maximumTime));
        callIds.Add(0); // שמירת ה-ID ברשימה
    }
}


    public static void creatAssignment()
    {
        var allCalls = s_dal!.call.ReadAll();
        var allVolunteers = s_dal!.volunteer.ReadAll();

        // Track assigned calls to ensure each call appears in only one assignment
        var assignedCalls = new HashSet<int>();

        // Remove the last two volunteers from the list
        var availableVolunteers = allVolunteers.Take(allVolunteers.Count() - 2).ToList();

        // Define the latest valid date (31 December 2024)
        DateTime maxValidDate = new DateTime(2024, 12, 31, 23, 59, 59);

        // Generate assignments for approximately one-third of the calls
        foreach (var call in allCalls)
        {
            // יוזמה יזומה: דילוג על קריאות בהסתברות של 2/3
            if (s_rand.Next(0, 3) != 0) // הסתברות של 2 מתוך 3 לדלג
                continue;

            // בחירת מתנדב אקראי מתוך המתנדבים הזמינים
            int volunteerIndex = s_rand.Next(0, availableVolunteers.Count());
            var selectedVolunteer = availableVolunteers[volunteerIndex];

            DateTime treatmentStartTime = call.startTime.Value.AddHours(s_rand.Next(1, 36));

            if (call.maximumTime.HasValue)
            {
                treatmentStartTime = treatmentStartTime < call.maximumTime.Value
                    ? treatmentStartTime
                    : call.maximumTime.Value.AddMinutes(-20);
            }

            if (treatmentStartTime > maxValidDate)
            {
                treatmentStartTime = maxValidDate;
            }

            // הגדרת זמן סיום הטיפול
            DateTime? treatmentEndTime = null;

            // בחירת סוג הטיפול (Hamal) אקראי
            Hamal assignKind = (Hamal)s_rand.Next(0, 3);

            switch (assignKind)
            {
                case Hamal.handeled:
                    treatmentEndTime = treatmentStartTime.AddHours(s_rand.Next(1, 24));
                    break;
                case Hamal.cancelByVolunteer:
                case Hamal.cancelByManager:
                    treatmentEndTime = null; // ביטול – אין זמן סיום
                    break;
            }

            // הוספת הקריאה לרשימת הקריאות שהוקצו
            assignedCalls.Add(call.id);

            // יצירת השמה חדשה
            s_dal!.assignment.Create(new Assignment(
                0, // מזהה יוקצה אוטומטית
                call.id,
                selectedVolunteer.idVol,
                treatmentStartTime,
                treatmentEndTime,
                assignKind));
        }
    }

    public static double[]? GetCoordinatesFromGoogle(string address)
    {
        // Step 1: Validate the input
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));

        // Step 2: Define the API key and base URL
        // TODO: Replace "YOUR_API_KEY" with your actual Google Maps API key.
        string apiKey = "AIzaSyDnyS5QMBa_4uwPOdbFH9T8_zNOXe3DzGw"; // Enter your Google Maps API key here.
        string apiUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

        // Step 3: Create an HttpClient to perform the request
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Step 4: Send the HTTP request synchronously
                HttpResponseMessage response = client.GetAsync(apiUrl).Result;

                // Step 5: Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Step 6: Read the response content synchronously
                string responseBody = response.Content.ReadAsStringAsync().Result;
                // Step 7: Parse the JSON response to extract latitude and longitude
                return ParseCoordinatesFromGoogle(responseBody);
            }
            catch (HttpRequestException)
            {
                // Handle cases like no internet or server unreachable
                throw new Exception("Unable to connect to the internet or server is unreachable.");
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                throw new Exception("An error occurred while fetching coordinates.", ex);
            }
        }
    }

    /// <summary>
    /// Parses the JSON response from Google Maps API and extracts the latitude and longitude.
    /// </summary>
    /// <param name="responseBody">The JSON response from Google Maps API.</param>
    /// <returns>
    /// An array containing the latitude and longitude if found, null otherwise.
    /// </returns>
    private static double[]? ParseCoordinatesFromGoogle(string responseBody)
    {
        // Parse the JSON response into a JObject
        JObject json = JObject.Parse(responseBody);

        // Check if the response contains a valid result
        JToken results = json["results"];
        if (results != null && results.HasValues)
        {
            JToken location = results[0]["geometry"]["location"];
            return new double[]
            {
            (double)location["lat"],
            (double)location["lng"]
            };
        }

        // If no results were found, return null
        return null;
    }




    //public static void Do(Iassignment? dalAssign, Ivolunteer? dalVolunteer, Icall? dalCall, Iconfig? dalconfig)
    //public static void Do(Idal dal) //stage 2
    public static void Do() //stage 4
    {
        // הצבת הממשקים שהתקבלו בפרמטרים למשתנים פנימיים
        // s_dalvolunteer = dalVolunteer ?? throw new NullReferenceException("DAL volunteer cannot be null!");
        //s_dalCall = dalCall ?? throw new NullReferenceException("DAL call cannot be null!");
        //s_dalAssignment = dalAssign ?? throw new NullReferenceException("DAL assignment cannot be null!");
        //s_dalconfig = dalconfig ?? throw new NullReferenceException("DAL Config cannot be null!");
        //s_dal=dal??throw new NullReferenceException("DAL object cannot be null!");//stage 2
        s_dal = DalApi.Factory.Get;
        //// הוספת הקוד לוודא שהשעה תקינה לפני החישוב
        //if (s_dalconfig.clock == DateTime.MinValue)
        //{
        //    s_dalconfig.clock = new DateTime(2022, 1, 1); // שנה/חודש/יום חוקיים
        //}

        // הצגת הודעת התחלה
        Console.WriteLine("Reset Configuration values and List values...");

        // איפוס נתוני התצורה
        //  s_dalconfig.Reset(); // איפוס התצורה
        s_dal.ResetDB();
        // איפוס הרשימות
       // s_dalvolunteer.DeleteAll(); // מחיקת כל המתנדבים
        //s_dalCall.DeleteAll(); // מחיקת כל הקריאות
        //s_dalAssignment.DeleteAll(); // מחיקת כל המשימות

        // אתחול הרשימות
        Console.WriteLine("Initializing volunteers list ...");
        creatVolunteer(); // יצירת מתנדבים
        Console.WriteLine("Initializing call list ...");
        creatCall(); // יצירת קריאות
        Console.WriteLine("Initializing assignment list ...");
        creatAssignment(); // יצירת משימות
       
    }
}