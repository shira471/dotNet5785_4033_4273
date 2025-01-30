using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DalApi;
using DO;
using Newtonsoft.Json.Linq;
using BL.Helpers;
using Helpers;
using System.Numerics;
using System.Net;
using BO;

internal static class VolunteerManager
{
    private static int s_periodicCounter = 0;
    internal static ObserverManager Observers = new(); // Stage 5
    // Access to DAL
    private static Idal s_dal = Factory.Get; // Stage 4

    /// <summary>
    /// Updates volunteer statuses according to logic based on the system clock.
    /// </summary>
    /// <param name="oldClock">The previous clock.</param>
    /// <param name="newClock">The new clock.</param>
    internal static void PeriodicVolunteersUpdates(DateTime oldClock, DateTime newClock)
    {
        Thread.CurrentThread.Name = $"Periodic{++s_periodicCounter}"; //stage 7 (optional)

        List<DO.Volunteer> volunteers;

        // Lock for reading all volunteers, converting to concrete list to avoid deferred query execution
        lock (AdminManager.BlMutex) // Stage 7
        {
            volunteers = s_dal.volunteer.ReadAll().ToList(); // ToList() makes it a concrete list
        }

        List<int> updatedVolunteersIds = new List<int>(); // Collect IDs for notification

        foreach (var volunteer in volunteers)
        {
            // Example: Check if a volunteer has been inactive for a certain period
            if (!volunteer.isActive && (newClock - oldClock).Days > 30)
            {
                // Create a new copy of the volunteer with updated isActive property
                var updatedVolunteer = volunteer with { isActive = false };

                // Lock for updating the volunteer in DAL
                lock (AdminManager.BlMutex) // Stage 7
                {
                    s_dal.volunteer.Update(updatedVolunteer);
                }

                // Collect updated volunteer's ID for notification
                updatedVolunteersIds.Add(updatedVolunteer.idVol); // Stage 5
            }
        }

        //// Send notifications outside the lock for all updated volunteers
        //foreach (var volunteerId in updatedVolunteersIds)
        //{
        //    Observers.NotifyItemUpdated(volunteerId); // Stage 5
        //}

        //// Optionally, notify list updated if there were changes
        //if (updatedVolunteersIds.Any())
        //{
        //    Observers.NotifyListUpdated(); // Stage 5
        //}
        NotifyObservers(updatedVolunteersIds);
    }
    internal static void SimulateVolunteerActivity(DateTime startClock, DateTime endClock)
    {
        Thread.CurrentThread.Name = $"SimulationThread{++s_periodicCounter}"; //stage 7 (optional)

        List<DO.Volunteer> volunteers;

        // Lock for reading all volunteers, converting to concrete list to avoid deferred query execution
        lock (AdminManager.BlMutex) // Stage 7
        {
            volunteers = s_dal.volunteer.ReadAll().ToList(); // ToList() makes it a concrete list
        }

        List<int> updatedVolunteersIds = new List<int>(); // Collect IDs for notification

        // Perform simulation over time period
        for (DateTime currentTime = startClock; currentTime <= endClock; currentTime = currentTime.AddDays(1))
        {
            foreach (var volunteer in volunteers)
            {
                // Example: Deactivate volunteers based on custom criteria (e.g., time period or manual deactivation)
                if (volunteer.isActive && ShouldDeactivate(volunteer))
                {
                    // Create a new copy of the volunteer with updated isActive property
                    var updatedVolunteer = volunteer with { isActive = false };

                    // Lock for updating the volunteer in DAL
                    lock (AdminManager.BlMutex) // Stage 7
                    {
                        s_dal.volunteer.Update(updatedVolunteer);
                    }

                    // Collect updated volunteer's ID for notification
                    updatedVolunteersIds.Add(updatedVolunteer.idVol); // Stage 5
                }
            }
        }

        //// Send notifications outside the lock for all updated volunteers
        //foreach (var volunteerId in updatedVolunteersIds)
        //{
        //    Observers.NotifyItemUpdated(volunteerId); // Stage 5
        //}

        //// Optionally, notify list updated if there were changes
        //if (updatedVolunteersIds.Any())
        //{
        //    Observers.NotifyListUpdated(); // Stage 5
        //}
        NotifyObservers(updatedVolunteersIds);
    }

    // Custom logic for deciding whether to deactivate a volunteer
    private static bool ShouldDeactivate(DO.Volunteer volunteer)
    {
        // Example: Add your custom logic here. For now, we deactivate based on `isActive`.
        return true; // Placeholder: Change this to match your criteria
    }



    /// <summary>
    /// Gets the coordinates for a given address using Google Maps API.
    /// </summary>
    /// <param name="address">The address to geocode.</param>
    /// <returns>An array containing latitude and longitude if found, null otherwise.</returns>
    public static async Task<double[]> GetCoordinatesFromGoogleAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));

        string apiKey = "AIzaSyDnyS5QMBa_4uwPOdbFH9T8_zNOXe3DzGw"; // Replace with your Google Maps API key.
        string apiUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response;

                // Lock for sending HTTP requests
                lock (AdminManager.BlMutex) // Stage 7
                {
                    response = client.GetAsync(apiUrl).Result;
                }

                response.EnsureSuccessStatusCode();

                string responseBody;

                // Lock for reading the response content
                lock (AdminManager.BlMutex) // Stage 7
                {
                    responseBody = response.Content.ReadAsStringAsync().Result;
                }

                // Parse the response outside of lock
                return ParseCoordinatesFromGoogle(responseBody);
            }
            catch (HttpRequestException)
            {
                throw new Exception("Unable to connect to the internet or server is unreachable.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching coordinates.", ex);
            }
        }
    }

    /// <summary>
    /// Parses the JSON response from Google Maps API and extracts the latitude and longitude.
    /// </summary>
    /// <param name="responseBody">The JSON response from Google Maps API.</param>
    /// <returns>An array containing the latitude and longitude if found, null otherwise.</returns>
    private static double[]? ParseCoordinatesFromGoogle(string responseBody)
    {
        JObject json = JObject.Parse(responseBody);

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

        return null;
    }
   

    /// <summary>
    /// עדכון המשקיפים.
    /// </summary>
    private static void NotifyObservers(List<int> updatedVolunteersIds)
    {
        foreach (var volunteerId in updatedVolunteersIds)
        {
            try
            {
                Observers.NotifyItemUpdated(volunteerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying observer for Volunteer ID {volunteerId}: {ex.Message}");
            }
        }

        if (updatedVolunteersIds.Any())
        {
            try
            {
                Observers.NotifyListUpdated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying list observers: {ex.Message}");
            }
        }

    }

    private static readonly Random s_rand = new();
    private static int s_simulatorCounter = 0;

    internal static void SimulateVolunteers() // stage 7
    {
        Thread.CurrentThread.Name = $"Simulator{++s_simulatorCounter}";

        LinkedList<int> volunteersToUpdate = new(); // stage 7
        List<DO.Volunteer> doVolunteerList;

        lock (AdminManager.BlMutex) // stage 7
            doVolunteerList = s_dal.volunteer.ReadAll(st => st.isActive).ToList();

        foreach (var doVolunteer in doVolunteerList)
        {
            lock (AdminManager.BlMutex) // stage 7
            {
                BO.CallInProgress? callInProgress;
                lock (AdminManager.BlMutex)
                    callInProgress = converterFromDoToBoVolunteer(doVolunteer).CurrentCall;
                // אם אין למתנדב קריאה בטיפולו
                if (callInProgress == null)
                {
                    IEnumerable<BO.OpenCallInList> openCallsOfVolunteer = CallManager.GetOpenCallsByVolunteer(doVolunteer.idVol);
                    int openCalls = openCallsOfVolunteer.Count();
                    if (openCalls != 0 && s_rand.Next(0, 5) == 0)
                    {
                        int callId = openCallsOfVolunteer.Skip(s_rand.Next(0, openCalls)).First().Id;
                        CallManager.AssignCallToVolunteer(doVolunteer.idVol, callId);
                    }
                }
                else // אם למתנדב יש קריאה בטיפולו
                {
                    int VolunteerSpeed = s_rand.Next(5, 50);
                    var arrivingTime = TimeSpan.FromHours(callInProgress.DistanceFromVolunteer / VolunteerSpeed);
                    var handleTime = TimeSpan.FromMinutes(s_rand.Next(5, 30));
                    var totalTime = arrivingTime + handleTime;

                    if (callInProgress.OpenTime + totalTime >= callInProgress.MaxCloseTime)
                    {
                       
                    }
                    else
                    {
                        if (s_rand.Next(0, 10) == 0) // 10% סיכוי לביטול
                        {
                            CallManager.CancelCallAssignment(doVolunteer.idVol, callInProgress.Id, (BO.Role)doVolunteer.role);
                        }
                        else // 90% סיכוי לסגירה מוצלחת
                        {
                            CallManager.CloseCallAssignment(doVolunteer.idVol, callInProgress.Id);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// מחשב את הזמן המוערך לטיפול בקריאה, בהתבסס על מרחק וזמן רנדומלי נוסף
    /// </summary>
    /// <param name="volunteer"></param>
    /// <param name="call"></param>
    /// <returns></returns>
    private static TimeSpan CalculateEstimatedTime(DO.Volunteer volunteer, DO.Call call)
    {
        // חישוב מרחק (אפשר להוסיף כאן חישוב מרחק גיאוגרפי אם יש פונקציה קיימת)
        double distance = CallManager.CalculateDistance(
            (double)volunteer.latitude, (double)volunteer.longitude,
            (double)call.latitude, (double)call.longitude
        );

        // תוספת זמן רנדומלית בין 10 ל-30 דקות
        double randomMinutes = s_rand.Next(10, 30);

        // זמן מוערך: זמן מבוסס מרחק + זמן רנדומלי
        return TimeSpan.FromMinutes(distance / 10 + randomMinutes); // לדוגמה: 10 קמ"ש
    }
   internal static BO.Volunteer converterFromDoToBoVolunteer(DO.Volunteer doVolunteer)
    {

        return new BO.Volunteer
        {
           Id=doVolunteer.idVol,
           FullName=doVolunteer.name,
           Phone=doVolunteer.phoneNumber,
            Email=doVolunteer.email,
            Password=doVolunteer.password,
            Address=doVolunteer.adress,
            Latitude=doVolunteer.latitude,
            Longitude=doVolunteer.longitude,
            Role=(BO.Role)doVolunteer.role,
            IsActive=doVolunteer.isActive,
            MaxDistance=doVolunteer.limitDestenation,
            DistanceType=(BO.DistanceType)doVolunteer.distanceType,
        };
    }
}

