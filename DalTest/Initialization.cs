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
        string[] names = { "Dani Levy", "Eli Amar", "Yair Cohen", "Ariela Levin", "Dina Klein", "Shira Israelof" };
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
        "123 Main St, New York, NY 10001",
        "456 Oak Ave, San Francisco, CA 94102",
        "789 Maple Dr, Chicago, IL 60605",
        "101 Pine Rd, Los Angeles, CA 90001",
        "202 Birch Blvd, Miami, FL 33101",
        "303 Elm St, Dallas, TX 75201",
        "404 Cedar Ln, Seattle, WA 98101",
        "505 Willow Dr, Boston, MA 02110",
        "606 Ash St, Denver, CO 80202",
        "707 Birch St, Austin, TX 73301",
        "808 Oak Ln, Phoenix, AZ 85001",
        "909 Palm Blvd, Orlando, FL 32801",
        "1001 Walnut St, Houston, TX 77001",
        "1102 Redwood Ave, San Diego, CA 92101",
        "1203 Cherry Rd, Nashville, TN 37201",
        "1304 Pinehurst St, Salt Lake City, UT 84101",
        "1405 Aspen Dr, Portland, OR 97201",
        "1506 Spruce St, Minneapolis, MN 55101",
        "1607 Poplar Ave, Indianapolis, IN 46201",
        "1708 Cedar Ave, Philadelphia, PA 19103"
         };
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
    private static Hamal? GetRandomHamalValue()
    {
        Array values = Enum.GetValues(typeof(Hamal));
        return s_rand.Next(0, 2) == 1 ? (Hamal)values.GetValue(s_rand.Next(values.Length)) : (Hamal?)null;
    }
    //private static Role? GetRandomRoleValue()
    //{
    //    Array values = Enum.GetValues(typeof(Role));
    //    return s_rand.Next(0, 2) == 1 ? (Role)values.GetValue(s_rand.Next(values.Length)) : (Role?)null;
    //}

    public static void creatCall()
    {
        // Random instance
        Random s_rand = new Random();

        // Predefined lists
        string[] details = {
        "Call for assistance", "Emergency call", "Routine check-in",
        "Follow-up call", "Service request", "Technical support",
        "Customer inquiry", "Appointment reminder", "Complaint report",
        "Survey feedback"
    };
        string[] addresses = new string[]
            {
        "3327 Ridgewood St, Suite 300, Chicago, IL 60601",
        "2144 Woodfield Rd, San Francisco, CA 94102",
        "7783 High Street, Denver, CO 80202",
        "506 Wycliff Ave, Los Angeles, CA 90012",
        "1699 Cherry Hill Road, Philadelphia, PA 19104",
        "2751 Hemlock Ln, Miami, FL 33101",
        "8361 Elmwood Ave, Houston, TX 77006",
        "4319 Maplewood Dr, Boston, MA 02118",
        "5986 Chestnut Dr, Dallas, TX 75205",
        "1181 Lakeview Blvd, Seattle, WA 98109",
        "2482 Green Hills Rd, Phoenix, AZ 85018",
        "8937 Pinewood Ln, New York, NY 10027",
        "5059 Cascade St, Portland, OR 97201",
        "6767 Birchwood Dr, Atlanta, GA 30303",
        "4509 Tanglewood Rd, Chicago, IL 60607",
        "2158 Vine St, San Diego, CA 92103",
        "3933 Redwood Ave, Salt Lake City, UT 84103",
        "1022 Pine Ridge Rd, Las Vegas, NV 89109",
        "7144 Waterford Dr, Los Angeles, CA 90036",
        "2321 Cross Creek Rd, Austin, TX 73301"
            };

        // Use system clock from s_dalConfig.Clock
        DateTime systemClock = s_dal.config.clock;
        DateTime start = new DateTime(systemClock.Year - 2, 1, 1);
        int range = (systemClock - start).Days;

        // Loop through all predefined details
        foreach (string detail in details)
        {
            // Random ID for the call
            int id = s_rand.Next(1000, 9999);
            // הוספת מזהה הקריאה לרשימה
            callIds.Add(id);
            // מערך של 20 כתובות רנדומליות עבור קריאות
            
            // Random address
            string address = addresses[s_rand.Next(addresses.Length)];

            double[]? coordinates = GetCoordinatesFromGoogle(address);
            if (coordinates == null)
            {
                throw new Exception($"Could not fetch coordinates for address: {address}");
            }
            double latitude = coordinates[0];
            double longitude = coordinates[1];


            // Random call type (Hamal enum)
            Hamal? callType = GetRandomHamalValue();

            // Random start time and maximum time
            DateTime startTime = start.AddDays(s_rand.Next(range));
            DateTime maximumTime = startTime.AddMinutes(s_rand.Next(1, 60));

            // Create the call and save it in the database
            s_dal.call.Create(new Call(id, detail, address, latitude, longitude, callType, startTime, maximumTime));
        }
    }


    public static void creatAssignment()
    {
        Random s_rand = new Random();

        // שימוש במזהים שנשמרו
        foreach (int callId in callIds)
        {
            // Random ID for the assignment
            int id = s_rand.Next(1000, 9999);

            // בחירת מתנדב אקראי מתוך הרשימה
            int volunteerId = volunteerIds[s_rand.Next(volunteerIds.Count)];

            // Random start time
            DateTime systemNow = s_dal.config.clock;
            DateTime start = new DateTime(systemNow.Year - 2, 1, 1);
            int range = (int)(systemNow - start).TotalDays;
            DateTime startTime = start.AddDays(s_rand.Next(range));

            // Random finish time
            DateTime finishTime = startTime.AddMinutes(s_rand.Next(30, 121));

            // Random endOfAssign (Hamal enum)
            Hamal? endOfAssign = GetRandomHamalValue();

            // Create the assignment and save it in the database
            s_dal.assignment.Create(new Assignment(id, callId, volunteerId, startTime, finishTime, endOfAssign));
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