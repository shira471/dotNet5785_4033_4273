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
        s_dal!.volunteer.Create(new Volunteer(212314033, "jerusalem", "shira alfasi", "syrhlpsy7@gmail.com", "0584084178", "58850090Aa@", latitude, longitude, 10, true, Role.Manager, TypeDistance.air));
        // הוספת מזהה המתנדב לרשימה
        volunteerIds.Add(212314033);
        s_dal!.volunteer.Create(new Volunteer(325004273, "jerusalem", "ahuvi winkler", "a@gmail.com", "0507678050", "1234567Zz@", latitude, longitude, 10, true, Role.Manager, TypeDistance.air));
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
            bool isActive = s_rand.Next(0, 100) >= 33;

            
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

        private static string GenerateRandomPassword(int minLength, int maxLength)
        {
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*(),.?\\:|<>";

            // Ensure at least one of each required character type
            char upper = upperChars[s_rand.Next(upperChars.Length)];
            char lower = lowerChars[s_rand.Next(lowerChars.Length)];
            char digit = digits[s_rand.Next(digits.Length)];
            char special = specialChars[s_rand.Next(specialChars.Length)];

            // Determine the remaining length of the password
            int length = s_rand.Next(minLength, maxLength + 1);
            int remainingLength = length - 4; // We already selected 4 mandatory characters

            const string allChars = upperChars + lowerChars + digits + specialChars;
            char[] password = new char[length];

            // Place the mandatory characters
            password[0] = upper;
            password[1] = lower;
            password[2] = digit;
            password[3] = special;

            // Fill the rest randomly
            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[s_rand.Next(allChars.Length)];
            }

            // Shuffle the password to ensure randomness
            return new string(password.OrderBy(x => s_rand.Next()).ToArray());
        }
        // Helper method to get a random value of the Hamal enum, or null
        private static CallType? GetRandomHamalValue()
    {
        var validValues = Enum.GetValues(typeof(CallType))
                 .Cast<CallType>()
                 .Where(kind => kind != CallType.none)
                 .ToArray();

        return validValues[s_rand.Next(validValues.Length)];
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
        "Survey feedback", "Medical emergency assistance",
        "Lost child at park", "Car accident report", "House fire alert",
        "Tree blocking road", "Water pipe burst", "Animal rescue request",
        "Missing person report", "Flooded street cleanup",
        "Electric outage report"
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
        "12 HaNevi'im St, Jerusalem, Israel",
        "22 Mamilla Mall, Jerusalem, Israel",
        "7 Zion Square, Jerusalem, Israel",
        "16 Herzl St, Jerusalem, Israel",
        "40 Hillel St, Jerusalem, Israel",
        "14 Agron St, Jerusalem, Israel",
        "28 Emek Refaim St, Jerusalem, Israel",
        "4 David Remez St, Jerusalem, Israel",
        "20 HaNevi'im St, Jerusalem, Israel",
        "5 Hahistadrut St, Jerusalem, Israel",
        "9 Yitzhak Rabin Blvd, Jerusalem, Israel"
    };

        DateTime now = DateTime.Now;

        foreach (string detail in details)
        {
            // בחירת כתובת אקראית
            string address = addresses[s_rand.Next(addresses.Length)];

            // קבלת קורדינטות לכתובת
            double[]? coordinates = GetCoordinatesFromGoogle(address);
            if (coordinates == null)
            {
                throw new Exception($"Could not fetch coordinates for address: {address}");
            }

            double latitude = coordinates[0];
            double longitude = coordinates[1];

            // יצירת זמן התחלה של הקריאה (תמיד בעבר, אך לא רחוק מדי)
            DateTime startTime = now.AddDays(-s_rand.Next(1, 10)).AddHours(s_rand.Next(0, 24));

            // יצירת זמן מקסימום שתמיד יהיה בעתיד
            DateTime maximumTime;
            do
            {
                if (s_rand.Next(0, 2) == 0)
                {
                    // זמן סיום בין 1 ל-30 ימים בעתיד
                    maximumTime = now.AddDays(s_rand.Next(1, 30)).AddHours(s_rand.Next(0, 24));
                }
                else
                {
                    // זמן סיום אחרי תחילת הקריאה (בין 30 ל-180 דקות אחרי)
                    maximumTime = startTime.AddMinutes(s_rand.Next(30, 180));
                }
            }
            while (maximumTime <= now); // לוודא שהתוצאה היא בעתיד

            // יצירת הקריאה החדשה
            s_dal!.call.Create(new Call(
                0, // מזהה אוטומטי
                detail,
                address,
                latitude,
                longitude,
                GetRandomHamalValue(),
                startTime,
                maximumTime
            ));

            callIds.Add(0); // שמירת מזהה הקריאה
        }
    }


    public static void creatAssignment()
    {
        var allCalls = s_dal!.call.ReadAll().ToList(); // טעינת כל הקריאות
        var allVolunteers = s_dal!.volunteer.ReadAll().ToList(); // טעינת כל המתנדבים

        var assignedVolunteers = new HashSet<int>(); // מתנדבים שכבר שובצו
        var assignedCalls = new HashSet<int>(); // קריאות שכבר שובצו

        foreach (var call in allCalls)
        {
            // הסתברות של 20% בלבד לבצע השמה לקריאה זו
            if (s_rand.Next(0, 5) != 0) // 20% הסתברות (1 מתוך 5)
                continue;

            // סינון מתנדבים שעדיין לא שובצו לקריאות
            var availableVolunteers = allVolunteers.Where(v => !assignedVolunteers.Contains(v.idVol)).ToList();
            if (!availableVolunteers.Any()) // אם נגמרו המתנדבים, יציאה מהלולאה
                break;

            // בחירת מתנדב אקראי מתוך הרשימה הזמינה
            int volunteerIndex = s_rand.Next(availableVolunteers.Count);
            var selectedVolunteer = availableVolunteers[volunteerIndex];

            // יצירת תאריך התחלת טיפול
            DateTime treatmentStartTime = call.startTime.Value.AddHours(s_rand.Next(1, 12)); // עד 12 שעות אחרי תחילת הקריאה

            // הגבלת זמן תחילת הטיפול למקסימום הזמן המוגדר בקריאה
            if (call.maximumTime.HasValue && treatmentStartTime > call.maximumTime.Value)
            {
                treatmentStartTime = call.maximumTime.Value;
            }

            DateTime? treatmentEndTime = null;
            Hamal assignKind;

            if (s_rand.Next(0, 2) == 0) // הסתברות של 50% שהמשימה תסתיים
            {
                treatmentEndTime = treatmentStartTime.AddHours(s_rand.Next(1, 6)); // זמן סיום בין 1 ל-6 שעות
                if (call.maximumTime.HasValue && treatmentEndTime > call.maximumTime.Value)
                {
                    treatmentEndTime = call.maximumTime.Value; // ודא שזמן הסיום לא עובר את זמן הסיום של הקריאה
                }
                assignKind = Hamal.handeled; // המשימה הושלמה
            }
            else
            {
                if (s_rand.Next(0, 2) == 0) // הסתברות של 50% להיות "בטיפול"
                {
                    if (treatmentStartTime <= DateTime.Now)
                    {
                        assignKind = Hamal.inTreatment; // מוגדר כ"בטיפול"
                    }
                    else
                    {
                        assignKind = (Hamal)s_rand.Next((int)Hamal.cancelByVolunteer, (int)Hamal.handelExpired + 1);
                    }
                }
                else
                {
                    assignKind = (Hamal)s_rand.Next((int)Hamal.cancelByVolunteer, (int)Hamal.handelExpired + 1);
                }
            }

            // יצירת ההשמה
            s_dal!.assignment.Create(new Assignment(
                0, // מזהה אוטומטי
                call.id,
                selectedVolunteer.idVol,
                treatmentStartTime,
                treatmentEndTime,
                assignKind
            ));

            // הוספת הקריאה והמתנדב לרשימות המעקב כדי למנוע השמה כפולה
            assignedCalls.Add(call.id);
            assignedVolunteers.Add(selectedVolunteer.idVol);
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