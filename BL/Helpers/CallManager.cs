using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using DalApi;
using DO;
using Helpers;
using BlImplementation;
using BO;
using BlApi;

namespace BL.Helpers;

internal static class CallManager
{
    private static int s_periodicCounter = 0;
    internal static ObserverManager Observers = new(); // Stage 5
    // Access to DAL
    private static Idal s_dal = DalApi.Factory.Get; // Stage 4

    /// <summary>
    /// Update calls according to the logic, based on the system clock.
    /// </summary>
    /// <param name="oldClock">The previous clock.</param>
    /// <param name="newClock">The new clock.</param>

    internal static void SimulateCallActivity(DateTime startClock, DateTime endClock)
    {
        Thread.CurrentThread.Name = $"SimulationThread{++s_periodicCounter}"; // Optional for debugging

        List<DO.Call> calls;

        // Lock for reading all calls, converting to concrete list to avoid deferred query execution
        lock (AdminManager.BlMutex) // Lock for DAL access
        {
            calls = s_dal.call.ReadAll().ToList(); // Convert to a concrete list
        }

        List<int> updatedCallIds = new List<int>(); // Collect IDs for notification

        // Perform simulation over the time period
        for (DateTime currentTime = startClock; currentTime <= endClock; currentTime = currentTime.AddDays(1))
        {
            foreach (var call in calls)
            {
                // Example: Automatically close calls if they exceed the maximum time
                if (call.maximumTime.HasValue && currentTime > call.maximumTime)
                {
                    var updatedCall = call with { maximumTime = null }; // Example: Set maximumTime to null to indicate closure

                    // Lock for updating the call in DAL
                    lock (AdminManager.BlMutex) // Lock per update
                    {
                        s_dal.call.Update(updatedCall);
                    }

                    // Collect the updated call's ID for notification
                    updatedCallIds.Add(updatedCall.id);
                }
            }

        }

        // Notify observers outside the lock
        foreach (var callId in updatedCallIds)
        {
            Observers.NotifyItemUpdated(callId);
        }

        // Optionally, notify that the whole list was updated if there were changes
        if (updatedCallIds.Any())
        {
            Observers.NotifyListUpdated();
        }
    }
    internal static void PeriodicCallUpdates(DateTime oldClock, DateTime newClock)
    {
        // Set thread name for easier debugging
        // Thread.CurrentThread.Name = $"PeriodicCallUpdates"; ??? to review

        // Local list to store IDs for notifications outside the lock
        List<int> expiredCallIds = new();

        // Step 1: Retrieve all calls from the data source
        List<DO.Call> activeCalls;
        lock (AdminManager.BlMutex) // Lock for data retrieval
        {
            activeCalls = s_dal.call.ReadAll()
                                   .Where(call => call.maximumTime > oldClock && call.maximumTime <= newClock)
                                   .ToList();
        }

        // Step 2: Process calls and perform updates
        foreach (var call in activeCalls)
        {

            // Add the call ID to the local list for notifications
            expiredCallIds.Add(call.id);
        }

        // Step 3: Send notifications outside the lock
        foreach (var callId in expiredCallIds)
        {
            Observers.NotifyItemUpdated(callId); // Notify about the specific updated item
        }

        // Step 4: Check if the list requires a global update notification
        if (oldClock.Year != newClock.Year || expiredCallIds.Any())
        {
            Observers.NotifyListUpdated(); // Notify about a global list update
        }
    }

    internal static void AssignCallToVolunteer(int volunteerId, int callId)
    {
        try
        {
            DO.Call call;
            DO.Volunteer volunteer;
            DO.Assignment volunteerCancelledAssignment;
            DO.Assignment existingAssignment;
            DO.Assignment volunteerActiveAssignment;
            // ✅ נעילה אחת לכל הגישות ל-DAL
            lock (AdminManager.BlMutex)
            {
                // שליפת הקריאה
                call = s_dal.call.Read(callId);

                // שליפת המתנדב
                volunteer = s_dal.volunteer.Read(volunteerId);

                // בדיקה אם המתנדב כבר ביטל את הקריאה בעבר
                volunteerCancelledAssignment = s_dal.assignment
                    .ReadAll()
                    .FirstOrDefault(a => a.callId == callId && a.volunteerId == volunteerId && a.assignKind == DO.Hamal.cancelByVolunteer);

                // בדיקה אם הקריאה כבר משויכת למתנדב אחר
                existingAssignment = s_dal.assignment
                    .ReadAll()
                    .FirstOrDefault(a => a.callId == callId &&
                                         a.assignKind != DO.Hamal.cancelByManager &&
                                         (a.assignKind == DO.Hamal.inTreatment || a.assignKind == DO.Hamal.handeled));

                volunteerActiveAssignment = s_dal.assignment
                                                         .ReadAll()
                                                           .FirstOrDefault(a => a.volunteerId == volunteerId &&
                                                         a.assignKind == DO.Hamal.inTreatment);
            }

            // 🔒 כל ה-throw נמצאים מחוץ ל-lock
            if (call == null)
                throw new Exception($"Call with ID={callId} does not exist.");

            if (volunteer == null)
                throw new Exception($"Volunteer with ID={volunteerId} does not exist.");

            if (volunteer.isActive == false) throw new Exception($"Volunteer with ID={volunteerId} not active");

            if (volunteerCancelledAssignment != null)
                throw new Exception($"Volunteer with ID={volunteerId} has already cancelled this call and cannot reassign it.");
            if (volunteerActiveAssignment != null) throw new Exception($"Volunteer with ID ={volunteerId} Handles another call");

                if (existingAssignment != null)
                throw new Exception($"Call with ID={callId} is already assigned to another volunteer.");

            // ✅ חישוב מרחק מחוץ ל-lock (אין צורך בנעילה עבור פונקציה חישובית)
            var distance = CalculateDistance(call.latitude ?? 0, call.longitude ?? 0, volunteer.latitude, volunteer.longitude);

            if (distance > volunteer.limitDestenation)
                throw new Exception($"Call is out of volunteer's range (Distance: {distance} > Limit: {volunteer.limitDestenation}).");

            // ✅ יצירת השמה חדשה - נעילת ה-DAL רק בעת יצירה
            var assignment = new DO.Assignment
            {
                callId = callId,
                volunteerId = volunteerId,
                startTime = AdminManager.Now,
                assignKind = DO.Hamal.inTreatment
            };

            lock (AdminManager.BlMutex)
            {
                s_dal.assignment.Create(assignment);
            }

            // ✅ עדכון סטטוס הקריאה
            var x = ConvertToBOCall(call);
            x.Status = Status.inProgres;

            CallManager.Observers.NotifyListUpdated();
            VolunteerManager.Observers.NotifyListUpdated();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    internal static BO.Call ConvertToBOCall(DO.Call doCall)
    {
        return new BO.Call
        {
            Id = doCall.id,
            CallType = (BO.CallType)(doCall.callType ?? 0), // המרה מסוג אם צריך
            Description = doCall.detail,
            Address = doCall.adress,
            Latitude = doCall.latitude,
            Longitude = doCall.longitude,
            OpenTime = doCall.startTime ?? DateTime.MinValue,
            MaxEndTime = doCall.maximumTime

        };
    }
    internal static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // פונקציה בסיסית לחישוב מרחק גיאוגרפי
        double R = 6371; // Earth's radius in km
        double dLat = DegreesToRadians(lat2 - lat1);
        double dLon = DegreesToRadians(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
    internal static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);
    internal static bool IsVolunteerBusy(int volunteerId)
    {
        List<DO.Assignment> assignments;
        DO.Volunteer v;

        // שימוש ב-lock אחד כדי להבטיח גישה בטוחה
        lock (AdminManager.BlMutex)
        {
            v = s_dal.volunteer.Read(volunteerId); // קריאת המתנדב
            assignments = s_dal.assignment.ReadAll()
                            .Where(a => a.volunteerId == volunteerId && a.assignKind == null)
                            .ToList(); // טוען את כל הנתונים לזיכרון
        }

        return assignments.Any(); // חיפוש מחוץ ל-lock, בטוח כעת
    }
    internal static IEnumerable<OpenCallInList> GetOpenCallsByVolunteer(int volunteerId, BO.CallType? callType = null, Enum? sortField = null)
    {
        List<DO.Call> calls;
        List<DO.Assignment> assignments;
        DO.Volunteer volunteer;

        lock (AdminManager.BlMutex)
        {
            // טעינת כל הקריאות שעדיין פתוחות (לא הסתיימו)
            calls = s_dal.call.ReadAll(c => c.maximumTime > DateTime.Now).ToList();

            // טעינת כל השיוכים
            assignments = s_dal.assignment.ReadAll().ToList();

            // טעינת פרטי המתנדב
            volunteer = s_dal.volunteer.ReadAll().FirstOrDefault(v => v.idVol == volunteerId);
        }

        // בדיקה אם המתנדב נמצא
        if (volunteer == null)
        {
            throw new InvalidOperationException("Volunteer not found.");
        }

        // בדיקה אם למתנדב יש נתוני מיקום
        if (volunteer.latitude == null || volunteer.longitude == null)
        {
            throw new ArgumentException("Volunteer location is not provided.");
        }

        // כעת ניתן לבצע את השאילתה מחוץ ל-lock, הנתונים נטענו מראש
        var openCalls = from call in calls
                        let callAssignments = assignments.Where(a => a.callId == call.id).ToList()
                        where
                            !callAssignments.Any() || // אין שיוכים כלל
                            callAssignments.All(a =>
                                a.assignKind == DO.Hamal.cancelByManager ||
                                a.assignKind == DO.Hamal.cancelByVolunteer) // כל השיוכים מבוטלים
                        select new OpenCallInList
                        {
                            Id = call.id,
                            Tkoc = (TheKindOfCall)(call.callType ?? 0),
                            Description = call.detail,
                            Address = call.adress,
                            OpenTime = call.startTime ?? DateTime.MinValue,
                            MaxEndTime = call.maximumTime,
                            DistanceFromVolunteer = CalculateDistance(call.latitude ?? 0, call.longitude ?? 0, volunteer.latitude, volunteer.longitude)
                        };

        // ✅ סינון לפי סוג הקריאה
        if (callType is not null and not BO.CallType.None)
        {
            openCalls = openCalls.Where(c => c.Tkoc == (TheKindOfCall)callType);
        }

        // ✅ מיון לפי שדה נבחר
        if (sortField is not null)
        {
            openCalls = sortField switch
            {
                SortField.Id => openCalls.OrderBy(call => call.Id),
                SortField.Address => openCalls.OrderBy(call => call.Address),
                SortField.OpenTime => openCalls.OrderBy(call => call.OpenTime),
                SortField.MaxFinishTime => openCalls.OrderBy(call => call.MaxEndTime),
                SortField.DistanceOfCall => openCalls.OrderBy(call => call.DistanceFromVolunteer),
                _ => openCalls.OrderBy(call => call.Id) // ברירת מחדל
            };
        }

        return openCalls;
    }
    internal static void CancelCallAssignment(int volunteerId, int callId, BO.Role role)
    {

        List<DO.Assignment> assignments;
        DO.Call call;

        // שימוש בנעילה לגישה ל-DAL
        lock (AdminManager.BlMutex)
        {
            // בדיקת השיוך
            assignments = s_dal.assignment.ReadAll()
                .Where(a => a.volunteerId == volunteerId
                            && a.callId == callId
                            && a.assignKind != DO.Hamal.cancelByVolunteer
                            && a.assignKind != DO.Hamal.cancelByManager)
                .ToList();

        }
        if (!assignments.Any())
        {
            throw new Exception($"No assignment found for Volunteer ID={volunteerId} and Call ID={callId}.");
        }

        if (assignments.Count > 1)
        {
            throw new Exception($"Multiple assignments found for Volunteer ID={volunteerId} and Call ID={callId}.");
        }

        // בדיקת הקריאה
        call = s_dal.call.Read(callId) ??
            throw new Exception($"Call with ID={callId} does not exist.");


            var assign = assignments.First();

            // קביעת סוג הביטול בהתאם לתפקיד
            BO.Hamal newAssignKind = role switch
            {
                BO.Role.Manager => BO.Hamal.cancelByManager,
                BO.Role.Volunteer => BO.Hamal.cancelByVolunteer,
                _ => throw new Exception($"Role {role} is not authorized to cancel assignments.")
            };

            // שימוש בנעילה לעדכון ב-DAL
            lock (AdminManager.BlMutex)
            {
                var updatedAssignment = assign with
                {
                    assignKind = (DO.Hamal)newAssignKind
                };
                s_dal.assignment.Update(updatedAssignment);
            }

            // עדכון סטטוס הקריאה
            var x = ConvertToBOCall(call);
            x.Status = Status.open;

            // עדכון צופים מחוץ לנעילה
            CallManager.Observers.NotifyListUpdated(); // שלב 5
        }
 
    internal static void CloseCallAssignment(int volunteerId, int callId)
    {
        List<DO.Assignment> assignments;
        DO.Call call;

        // קריאה ל-DAL עם נעילה
        lock (AdminManager.BlMutex)
        {
            // בדיקת השיוך
            assignments = s_dal.assignment.ReadAll()
                .Where(a => a.volunteerId == volunteerId
                            && a.callId == callId
                            && a.assignKind != DO.Hamal.cancelByVolunteer
                            && a.assignKind != DO.Hamal.cancelByManager)
                .ToList();

        }
        if (!assignments.Any())
        {
            throw new Exception($"No assignment found for Volunteer ID={volunteerId} and Call ID={callId}.");
        }

        // בדיקת שיוכים מרובים
        if (assignments.Count > 1)
        {
            throw new Exception($"Multiple assignments found for Volunteer ID={volunteerId} and Call ID={callId}.");
        }

        // בדיקת סטטוס השיוך
        var assign = assignments.First();
        if (assign.assignKind != DO.Hamal.inTreatment)
        {
            throw new Exception($"Assignment for Volunteer ID={volunteerId} and Call ID={callId} has already been closed.");
        }
        var adminImplementation = new AdminImplementation();
        var systemClock = adminImplementation.GetSystemClock();
        // עדכון זמן סיום השיוך
        var updatedAssignment = assign with
        {
            finishTime = systemClock,
            assignKind = DO.Hamal.handeled
        };
        s_dal.assignment.Update(updatedAssignment);

        // שליפת הקריאה ועדכון סטטוס הקריאה
        call = s_dal.call.Read(callId) ??
            throw new Exception($"Call with ID={callId} does not exist.");


        // עדכון סטטוס הקריאה
        var x = ConvertToBOCall(call);
        x.Status = Status.closed;
        CallManager.Observers.NotifyListUpdated(); // שלב 5
    }
    internal static void UpdateCallDetails(BO.Call call)
    {
        // וידוא שהקריאה אינה null
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call), "Call cannot be null.");
        }
        if (call.CallType == BO.CallType.None)
            throw new ArgumentException("Call type cannot be None.");
        // בדיקת תקינות לוגית
        if (call.MaxEndTime <= call.OpenTime)
        {
            throw new ArgumentException("MaxEndTime must be greater than OpenTime.");
        }
        if (call.MaxEndTime != null)
        {
            throw new ArgumentException("MaxEndTime cannot be null.");
        }
        var existingCall = s_dal.call.Read(call.Id)
        ?? throw new Exception($"Call with ID {call.Id} not found.");

        bool addressChanged = existingCall.adress != call.Address;
        var doCall = new DO.Call
        {
            id = call.Id,
            detail = call.Description,
            adress = call.Address,
            latitude = existingCall.latitude, // שימוש בקואורדינטות הקיימות עד לעדכון מאוחר יותר
            longitude = existingCall.longitude,
            callType = (DO.CallType?)call.CallType,
            startTime = call.OpenTime,
            maximumTime = call.MaxEndTime
        };

        // עדכון הקריאה בשכבת ה-DAL
        try
        {
            lock (AdminManager.BlMutex) // שלב 7
            {
                s_dal.call.Update(doCall);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to update call.", ex);
        }


        // עדכון תצפיתנים (מחוץ לנעילה)
        CallManager.Observers.NotifyItemUpdated(call.Id);
        CallManager.Observers.NotifyListUpdated();

        // ✔ חישוב קואורדינטות ברקע *רק אם הכתובת שונתה*
        if (addressChanged)
        {
            _ = Task.Run(() => UpdateCallCoordinatesAsync(doCall));
        }
    }
    private static readonly SemaphoreSlim _dbLock = new SemaphoreSlim(1, 1);
    public static async Task UpdateCallCoordinatesAsync(DO.Call doCall)
    {
        if (string.IsNullOrWhiteSpace(doCall.adress))
            return;

        try
        {
            var coords = await VolunteerManager.GetCoordinatesFromGoogleAsync(doCall.adress);

            if (coords != null)
            {
                doCall = doCall with { latitude = coords[0], longitude = coords[1] };

                await _dbLock.WaitAsync(); // נעילה אסינכרונית
                try
                {
                    s_dal.call.Update(doCall);
                }
                finally
                {
                    _dbLock.Release(); // שחרור הנעילה
                }

                // עדכון הצופים שהתהליך הושלם
                CallManager.Observers.NotifyItemUpdated(doCall.id);
                CallManager.Observers.NotifyListUpdated();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ שגיאה בחישוב קואורדינטות: {ex.Message}");
        }
    }
}