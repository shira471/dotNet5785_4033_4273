using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DalApi;
using DO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


using BL.Helpers;
using Helpers;


internal static class VolunteerManager
{
    internal static ObserverManager Observers = new(); //stage 5 
    // גישה לשכבת ה-DAL
    private static Idal s_dal = Factory.Get; //stage 4

    /// <summary>
    /// עדכון מצב מתנדבים על פי לוגיקה בהתאם לשעון המערכת.
    /// </summary>
    /// <param name="oldClock">השעון הקודם</param>
    /// <param name="newClock">השעון החדש</param>
    internal static void PeriodicVolunteersUpdates(DateTime oldClock, DateTime newClock)
    {
        var volunteers = s_dal.volunteer.ReadAll().ToList();

        foreach (var volunteer in volunteers)
        {
            // דוגמה: בדיקה אם מתנדב לא פעיל במשך זמן מסוים
            if (!volunteer.isActive && (newClock - oldClock).Days > 30)
            {
                // יצירת עותק חדש של המתנדב עם עדכון isActive
                var updatedVolunteer = volunteer with { isActive = false };
                s_dal.volunteer.Update(updatedVolunteer); // עדכון ב-DAL
                Observers.NotifyItemUpdated(updatedVolunteer.idVol); //stage 5
            }
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
}
