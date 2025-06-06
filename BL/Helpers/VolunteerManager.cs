﻿using System;
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
using BlImplementation;
using System.Text.RegularExpressions;

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
        List<DO.Assignment> assigmnet;
        // Lock for reading all volunteers, converting to concrete list to avoid deferred query execution
        lock (AdminManager.BlMutex) // Stage 7
        {
            volunteers = s_dal.volunteer.ReadAll().ToList(); // ToList() makes it a concrete list
            assigmnet = s_dal.assignment.ReadAll().ToList();
        }

        List<int> updatedVolunteersIds = new List<int>(); // Collect IDs for notification
       
        foreach (var volunteer in volunteers)
        {
            // שליפת ההשמה האחרונה של המתנדב
            var lastAssignment = assigmnet
                .Where(a => a.volunteerId == volunteer.idVol)
                .OrderByDescending(a => a.finishTime ?? DateTime.MinValue) // מסדר לפי הזמן האחרון שהסתיימה
                .FirstOrDefault();

            // אם אין השמות, מדלגים
            if (lastAssignment == null)
                continue;
            bool isClosedOrCancelled = lastAssignment.assignKind == DO.Hamal.cancelByManager ||
                                 lastAssignment.assignKind == DO.Hamal.cancelByVolunteer ||
                                 lastAssignment.assignKind == DO.Hamal.handeled;

            // בדיקה אם עברו יותר מ-30 יום מאז שהסתיימה או בוטלה
            if (isClosedOrCancelled && lastAssignment.finishTime.HasValue &&
                (newClock - lastAssignment.finishTime.Value).Days > 30)
            {
                // אם המתנדב כבר לא פעיל, אין צורך לעדכן שוב
                if (!volunteer.isActive)
                    continue;

                // יצירת עותק חדש של המתנדב עם `isActive = false`
                var updatedVolunteer = volunteer with { isActive = false };

                // נעילה ועדכון המתנדב ב-DAL
                lock (AdminManager.BlMutex)
                {
                    s_dal.volunteer.Update(updatedVolunteer);
                }

                // הוספת ה-ID של המתנדב לרשימת העדכונים
                updatedVolunteersIds.Add(updatedVolunteer.idVol);
            }
        }

        NotifyObservers(updatedVolunteersIds);
    }
    internal static void SimulateVolunteerActivity(DateTime startClock, DateTime endClock)
    {
        Thread.CurrentThread.Name = $"SimulationThread{++s_periodicCounter}"; //stage 7 (optional)
        PeriodicVolunteersUpdates(startClock, endClock);
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
    private static readonly SemaphoreSlim _httpLock = new SemaphoreSlim(1, 1);
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<double[]> GetCoordinatesFromGoogleAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));

        string apiKey = "AIzaSyDnyS5QMBa_4uwPOdbFH9T8_zNOXe3DzGw"; // Replace with your Google Maps API key.
        string apiUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

        await _httpLock.WaitAsync(); // נעילה אסינכרונית כדי למנוע קריאות מקבילות בעייתיות
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
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
        finally
        {
            _httpLock.Release(); // שחרור המנעול
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
            if (AdminManager.BlMutex != null)
            {
                    BO.CallInProgress? callInProgress;
                lock (AdminManager.BlMutex)
                { 
                    callInProgress = converterFromDoToBoVolunteer(doVolunteer).CurrentCall;
                }
                // אם אין למתנדב קריאה בטיפולו
                if (callInProgress == null)
                    {
                        IEnumerable<BO.OpenCallInList> openCallsOfVolunteer = CallManager.GetOpenCallsByVolunteer(doVolunteer.idVol);
                        int openCalls = openCallsOfVolunteer.Count();
                        if (openCalls != 0 && s_rand.Next(0, 5) == 0)
                        {
                            int callId = openCallsOfVolunteer.Skip(s_rand.Next(0, openCalls)).First().Id;
                        try
                        {
                                CallManager.AssignCallToVolunteer(doVolunteer.idVol, callId);
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        }
                    }
                    else // אם למתנדב יש קריאה בטיפולו
                    {
                    int VolunteerSpeed = s_rand.Next(10, 60); // מהירות הליכה בקמ"ש
                    var arrivingTime = TimeSpan.FromHours(callInProgress.DistanceFromVolunteer / VolunteerSpeed);
                        var handleTime = TimeSpan.FromMinutes(s_rand.Next(5, 30));
                        var totalTime = arrivingTime + handleTime;


                    if (callInProgress.OpenTime + totalTime > callInProgress.MaxCloseTime)
                    {
                        CallManager.CancelCallAssignment(doVolunteer.idVol, callInProgress.CallId, BO.Role.Manager);
                    }
                    else
                    {
                        // 95% סיכוי לסגירה מוצלחת, 5% סיכוי לביטול
                        if (s_rand.Next(0, 100) < 95)  // 95% מהפעמים
                        {
                            CallManager.CloseCallAssignment(doVolunteer.idVol, callInProgress.CallId);
                        }
                        else // 5% סיכוי לביטול מוצלחת
                        {
                            CallManager.CancelCallAssignment(doVolunteer.idVol, callInProgress.CallId, (BO.Role)doVolunteer.role);

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
        var callImplementation = new CallImplementation();
        var assignedCall = callImplementation.GetActiveAssignmentForVolunteer(doVolunteer.idVol); // מחזיר BO.CallInProgres
        return new BO.Volunteer
        {
            Id = doVolunteer.idVol,
            FullName = doVolunteer.name,
            Phone = doVolunteer.phoneNumber,
            Email = doVolunteer.email,
            Password = doVolunteer.password,
            Address = doVolunteer.adress,
            Latitude = doVolunteer.latitude,
            Longitude = doVolunteer.longitude,
            Role = (BO.Role)doVolunteer.role,
            IsActive = doVolunteer.isActive,
            MaxDistance = doVolunteer.limitDestenation,
            DistanceType = (BO.DistanceType)doVolunteer.distanceType,
            CurrentCall = assignedCall
         };
    }

    internal static void UpdateVolunteer(int requesterId, BO.Volunteer volunteer)
    {
        if (volunteer == null)
        {
            throw new BlNullPropertyException("Volunteer object cannot be null.");
        }
       
        // וידוא פורמט תקין של המייל, הטלפון וה-ID
        ValidateVolunteer(volunteer);
        DO.Volunteer existingVolunteer;
        lock (AdminManager.BlMutex)
        {
            // בדיקה אם המתנדב קיים במערכת
            existingVolunteer = s_dal.volunteer.Read(volunteer.Id);
        }

        // בדיקה לאחר ה-lock
        if (existingVolunteer == null)
        {
            throw new BlDoesNotExistException($"Volunteer with ID {volunteer.Id} not found.");
        }

        bool addressChanged = existingVolunteer.adress != volunteer.Address;

        try
        {
            IsStrongPassword(volunteer.Password);
            lock (AdminManager.BlMutex)  // נעילה סביב גישה ל-DAL
            {
                // יצירת המתנדב המעודכן
                var updatedVolunteer = new DO.Volunteer(
                    idVol: volunteer.Id,
                    adress: volunteer.Address,
                    name: volunteer.FullName,
                    email: volunteer.Email,
                    phoneNumber: volunteer.Phone,
                    password: volunteer.Password ?? existingVolunteer.password,  // אם אין סיסמה חדשה, שמירה על הישנה
                    latitude: existingVolunteer.latitude,
                    longitude: existingVolunteer.longitude,
                    limitDestenation: volunteer.MaxDistance ?? existingVolunteer.limitDestenation,
                    isActive: volunteer.IsActive,
                    role: (DO.Role?)volunteer.Role,
                    distanceType: (DO.TypeDistance?)volunteer.DistanceType
                );

                // עדכון המתנדב בשכבת ה-DAL
                s_dal.volunteer.Update(updatedVolunteer);

                // עדכון צופים
                VolunteerManager.Observers.NotifyItemUpdated(updatedVolunteer.idVol);
                VolunteerManager.Observers.NotifyListUpdated();
                // אם הכתובת השתנתה, חישוב הקואורדינטות ברקע
                if (existingVolunteer.adress != volunteer.Address)
                    _ = Task.Run(() => updateCoordinatesForVolunteerAsync(updatedVolunteer));
            }
        }
        catch (Exception ex)
        {
            throw new BlInvalidValueException($"Failed to update the volunteer details.{ex.Message}");
        }
    }

    internal static void ValidateVolunteer(BO.Volunteer volunteer)
    {
        if (!IsValidEmail(volunteer.Email))
        {
            throw new BlInvalidValueException("Invalid email format.");
        }

        if (!IsValidPhoneNumber(volunteer.Phone))
        {
            throw new BlInvalidValueException("Invalid phone number.");
        }

        if (!IsValidId(volunteer.Id))
        {
            throw new BlInvalidValueException("Invalid ID number.");
        }
    }

    internal static bool IsValidEmail(string email)
    {

        var mail = new System.Net.Mail.MailAddress(email);
        return mail.Address == email;
    }

    internal static bool IsValidPhoneNumber(string phoneNumber)
    {
        return phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 7;
    }

    internal static bool IsValidId(int id)
    {
        return id > 0 && id.ToString().Length == 9;
    }
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    internal static async Task updateCoordinatesForVolunteerAsync(DO.Volunteer doVolunteer)
    {

        if (!string.IsNullOrEmpty(doVolunteer.adress))
        {
            try
            {
                double[]? loc = await VolunteerManager.GetCoordinatesFromGoogleAsync(doVolunteer.adress);
                if (loc != null)
                {
                    doVolunteer = doVolunteer with { latitude = loc[0], longitude = loc[1] };

                    await _lock.WaitAsync(); // נעילה אסינכרונית
                    try
                    {
                        DalApi.Factory.Get.volunteer.Update(doVolunteer);
                    }
                    finally
                    {
                        _lock.Release(); // שחרור הנעילה
                    }

                    VolunteerManager.Observers.NotifyItemUpdated(doVolunteer.idVol);
                    VolunteerManager.Observers.NotifyListUpdated();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to fetch coordinates: {ex.Message}");
            }
        }


    }
    internal static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.");

        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long.");

        if (!password.Any(char.IsUpper))
            throw new ArgumentException("Password must contain at least one uppercase letter (A-Z).");

        if (!password.Any(char.IsLower))
            throw new ArgumentException("Password must contain at least one lowercase letter (a-z).");

        if (!password.Any(char.IsDigit))
            throw new ArgumentException("Password must contain at least one number (0-9).");

        // לפחות תו מיוחד
        if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?\:|<>]"))
            throw new ArgumentException("Password must contain at least one special character (!@#$%^&* etc.).");

        return true; // הסיסמה חזקה
    }
}

