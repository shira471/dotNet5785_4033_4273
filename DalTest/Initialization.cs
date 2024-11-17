
namespace DalTest;
using System.Buffers.Text;

using System;
using DalApi;
using DO;
using System.Xml.Linq;
using System.Text.RegularExpressions;

public static class Initialization
{
    private static Iassignment? s_dalAssignment; //stage 1
    private static Icall? s_dalCall; //stage 1
    private static Ivolunteer? s_dalvolunteer; //stage 1
    private static Iconfig? s_dalconfig; //stage 1
                                         // Random instance for generating values
    private static readonly Random s_rand = new Random();

private static void creatVolunteer()
    {
        Random s_rand = new Random();
        // Random name from a predefined list
        string[] names = { "Dani Levy", "Eli Amar", "Yair Cohen", "Ariela Levin", "Dina Klein", "Shira Israelof" };
        string name = names[s_rand.Next(names.Length)];

        // Random ID
        int id = s_rand.Next(1000000, 9999999); // Assuming 7-digit IDs

        // Random address
        string[] streets = { "Main St", "Second Ave", "Highland Rd", "Maple Dr", "Oak St" };
        string address = $"{s_rand.Next(1, 1000)} {streets[s_rand.Next(streets.Length)]}";

        // Random email
        string email = $"{name.Replace(" ", ".").ToLower()}@example.com";

        // Random phone number (10 digits)
        double phoneNumber = s_rand.Next(1000000000, int.MaxValue) + s_rand.NextDouble();

        // Random password (alphanumeric, length between 6 and 10)
        string password = GenerateRandomPassword(6, 10);

        // Random limit of destination (between 5 and 50 kilometers)
        double limitDestination = Math.Round(s_rand.NextDouble() * 45 + 5, 2);

        // Random active status
        bool isActive = s_rand.Next(0, 2) == 1;

        // Random latitude and longitude (within reasonable bounds for a location)
        double latitude = s_rand.NextDouble() * 180 - 90;  // Latitude between -90 and 90
        double longitude = s_rand.NextDouble() * 360 - 180;  // Longitude between -180 and 180

        // Random role and distance type (if the Hamal enum is defined)
        Hamal? role = GetRandomHamalValue();
        Hamal? distanceType = GetRandomHamalValue();

        // Create the volunteer with random values and save it in the database
        s_dalvolunteer!.Create(new Volunteer(id, address, name, email, phoneNumber, password, latitude, longitude, limitDestination, isActive, role, distanceType));
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
    private static void creatCall()
    {
        Random s_rand = new Random();
        // Random ID for the call
        int id = s_rand.Next(1000, 9999); // Assuming 4-digit IDs for calls

        // Random call details
        string[] details = { "Call for assistance", "Emergency call", "Routine check-in", "Follow-up call", "Service request" };
        string detail = details[s_rand.Next(details.Length)];

        // Random address
        string[] addresses = { "123 Main St", "456 Oak Ave", "789 Maple Dr", "101 Pine Rd", "202 Birch Blvd" };
        string address = addresses[s_rand.Next(addresses.Length)];

        // Random latitude and longitude (within reasonable bounds for a location)
        double latitude = s_rand.NextDouble() * 180 - 90;  // Latitude between -90 and 90
        double longitude = s_rand.NextDouble() * 360 - 180;  // Longitude between -180 and 180

        // Random call type (Hamal enum)
        Hamal? callType = GetRandomHamalValue(); // This will give either a Hamal value or null

        // Random start time (within the past month)
        DateTime start = new DateTime(s_dalconfig.clock.Year - 2, 1, 1); //stage 1
        int range = (s_dalconfig.clock - start).Days; //stage 1
        DateTime startTime= start.AddDays(s_rand.Next(range));
        DateTime maximumTime = start.AddMinutes(s_rand.Next(1, 60)); // Maximum time between 1 and 60 minutes after start time

        // Create the call with random values and save it in the database
        s_dalCall!.Create(new Call(id, detail, address, latitude, longitude, callType ,startTime, maximumTime));
    }
    private static void creatAssignment()
    {
       Random s_rand = new Random();
        // Random ID for the assignment
        int id = s_rand.Next(1000, 9999); // Assuming 4-digit IDs for assignments

        // Random callId (you may want to ensure this call ID exists)
        int callId = s_rand.Next(1000, 9999); // Replace with actual logic to get an existing call ID

        // Random volunteerId (you may want to ensure this volunteer ID exists)
        int volunteerId = s_rand.Next(1000, 9999); // Replace with actual logic to get an existing volunteer ID

        // Random start time (within the past 30 days)
        DateTime startTime = DateTime.Now.AddDays(s_rand.Next(-30, 1)); // Start within the last 30 days

        // Random finish time (between 30 minutes and 2 hours after start time)
        DateTime finishTime = startTime.AddMinutes(s_rand.Next(30, 121)); // 30 minutes to 2 hours later

        // Random endOfAssign (Hamal enum)
        Hamal? endOfAssign = GetRandomHamalValue(); // This gives either a Hamal value or null

        // Create the assignment with random values and save it in the database
        s_dalAssignment!.Create(new Assignment(id, callId, volunteerId, startTime, finishTime, endOfAssign));
    }

    public static void Do(Iassignment? dalAssign, Ivolunteer? dalVolunteer, Icall? dalCall, Iconfig? dalconfig)
    {
        // הצבת הממשקים שהתקבלו בפרמטרים למשתנים פנימיים
        s_dalvolunteer = dalVolunteer ?? throw new NullReferenceException("DAL volunteer cannot be null!");
        s_dalCall = dalCall ?? throw new NullReferenceException("DAL Course cannot be null!");
        s_dalAssignment = dalAssign ?? throw new NullReferenceException("DAL Link cannot be null!");
        s_dalconfig = dalconfig ?? throw new NullReferenceException("DAL Config cannot be null!");

        // הצגת הודעת התחלה
        Console.WriteLine("Reset Configuration values and List values...");

        // איפוס נתוני התצורה
        s_dalconfig.Reset(); // איפוס התצורה


        // איפוס הרשימות
        s_dalvolunteer.DeleteAll(); // מחיקת כל המתנדבים
        s_dalCall.DeleteAll(); // מחיקת כל הקריאות
        s_dalAssignment.DeleteAll(); // מחיקת כל המשימות

        // אתחול הרשימות
        Console.WriteLine("Initializing Students list ...");
        creatVolunteer(); // יצירת מתנדבים
        Console.WriteLine("Initializing Courses list ...");
        creatAssignment(); // יצירת משימות
        Console.WriteLine("Initializing Links list ...");
        creatCall(); // יצירת קריאות
    }
}


