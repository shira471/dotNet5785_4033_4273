namespace BlImplementation;

using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.Reflection.Metadata;
using BL.Helpers;
using BlApi;
using BO;
using DalApi;

using Helpers;


public class CallImplementation : ICall
{
    private static readonly DalApi.Idal _dal = DalApi.Factory.Get;


    //public async Task AddCall(Call call)
    //{
    //    AdminManager.ThrowOnSimulatorIsRunning(); // Stage 7
    //    if (call == null)
    //    {
    //        throw new ArgumentNullException(nameof(call), "Call cannot be null.");
    //    }

    //    if (call.CallType == CallType.None)
    //    {
    //        throw new ArgumentException("Call type cannot be None.");
    //    }

    //    if (call.MaxEndTime <= call.OpenTime)
    //    {
    //        throw new ArgumentException("End time cannot be earlier than start time.");
    //    }
    //    if (call.MaxEndTime == null)
    //    {
    //        throw new ArgumentException("End time cannot be null.");
    //    }
    //    try
    //    {
    //        var temp = await VolunteerManager.GetCoordinatesFromGoogleAsync(call.Address);

    //        lock (AdminManager.BlMutex) // Stage 7
    //        {
    //            var calls = _dal.call.ReadAll();
    //            foreach (var call2 in calls)
    //            {
    //                if (call2.detail == call.Description)
    //                {
    //                    throw new Exception("This call already exists.");
    //                }
    //            }

    //            var doCall = new DO.Call(
    //                id: 0, // ייווצר מזהה חדש ב-DAL
    //                detail: call.Description,
    //                adress: call.Address,
    //                latitude: temp[0],
    //                longitude: temp[1],
    //                callType: (DO.CallType?)call.CallType,
    //                startTime: call.OpenTime,
    //                maximumTime: call.MaxEndTime
    //            );

    //            _dal.call.Create(doCall);
    //            call.Status = Status.open;

    //            AdminImplementation admin = new AdminImplementation();
    //            UpdateStatus(call, admin.GetRiskTimeSpan());
    //        }

    //        // Notification to observers (outside lock)
    //        CallManager.Observers.NotifyListUpdated(); // Stage 5
    //    }
    //    catch (DO.DalAlreadyExistsException ex)
    //    {
    //        throw new Exception(ExceptionsManager.HandleException(new Exception("Failed to add call.", ex)));
    //    }
    //}
    public async Task AddCall(Call call)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7

        if (call == null)
            throw new ArgumentNullException(nameof(call), "Call cannot be null.");

        if (call.CallType == CallType.None)
            throw new ArgumentException("Call type cannot be None.");

        if (call.MaxEndTime <= call.OpenTime)
            throw new ArgumentException("End time cannot be earlier than start time.");

        if (call.MaxEndTime == null)
            throw new ArgumentException("End time cannot be null.");

        // ✔ שליחה מיידית ל-DAL *בלי* הקואורדינטות
        var doCall = new DO.Call(
            id: 0,
            detail: call.Description,
            adress: call.Address,
            latitude: null,  // ❌ עדיין לא מחשבים קואורדינטות!
            longitude: null,
            callType: (DO.CallType?)call.CallType,
            startTime: call.OpenTime,
            maximumTime: call.MaxEndTime
        );

        lock (AdminManager.BlMutex)
        {
            _dal.call.Create(doCall);
        }

        call.Status = Status.open;

        // ✔ נשלח את העדכון לאובזרברים (רשימות קריאות מתנדבים וכו')
        await Task.Run(() => CallManager.Observers.NotifyListUpdated());

        // ✔ חישוב קואורדינטות ברקע בלי לחכות
        _ = UpdateCallCoordinatesAsync(doCall);
    }

    // ✔ תת-מתודה אסינכרונית לחישוב הקואורדינטות ולשליחה מחדש ל-DAL
    private static async Task UpdateCallCoordinatesAsync(DO.Call doCall)
    {
        if (string.IsNullOrWhiteSpace(doCall.adress))
            return;

        try
        {
            var coords = await VolunteerManager.GetCoordinatesFromGoogleAsync(doCall.adress);

            if (coords != null)
            {
                // ✔ נעילה רק בזמן העדכון ב-DAL
                lock (AdminManager.BlMutex)
                {
                    doCall = doCall with { latitude = coords[0], longitude = coords[1] };
                    _dal.call.Update(doCall);
                }

                // ✔ עדכון הצופים שהתהליך הושלם
                CallManager.Observers.NotifyItemUpdated(doCall.id);
                CallManager.Observers.NotifyListUpdated();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ שגיאה בחישוב קואורדינטות: {ex.Message}");
        }
    }



    //public void AssignCallToVolunteer(int volunteerId, int callId)
    //{
    //    AdminManager.ThrowOnSimulatorIsRunning(); // Stage 7

    //    lock (AdminManager.BlMutex) // Stage 7
    //    {
    //        // שליפת הקריאה ממאגר הנתונים
    //        var call = _dal.call.Read(callId) ??
    //            throw new Exception($"Call with ID={callId} does not exist.");

    //        // שליפת המתנדב ממאגר הנתונים
    //        var volunteer = _dal.volunteer.Read(volunteerId) ??
    //            throw new Exception($"Volunteer with ID={volunteerId} does not exist.");

    //        // בדיקה אם המתנדב כבר ביטל את הקריאה בעבר
    //        var volunteerCancelledAssignment = _dal.assignment.ReadAll()
    //            .FirstOrDefault(a => a.callId == callId && a.volunteerId == volunteerId && a.assignKind == DO.Hamal.cancelByVolunteer);

    //        if (volunteerCancelledAssignment != null)
    //        {
    //            throw new Exception($"Volunteer with ID={volunteerId} has already cancelled this call and cannot reassign it.");
    //        }

    //        // בדיקה אם הקריאה כבר משויכת למתנדב אחר
    //        var existingAssignment = _dal.assignment.ReadAll()
    //            .FirstOrDefault(a => a.callId == callId &&
    //                                 a.assignKind != DO.Hamal.cancelByManager &&
    //                                 (a.assignKind == DO.Hamal.inTreatment || a.assignKind == DO.Hamal.handeled));

    //        if (existingAssignment != null)
    //        {
    //            throw new Exception($"Call with ID={callId} is already assigned to another volunteer.");
    //        }

    //        // בדיקה אם למתנדב יש קריאה אחרת במצב "בטיפול"
    //        var volunteerActiveAssignment = _dal.assignment.ReadAll()
    //            .FirstOrDefault(a => a.volunteerId == volunteerId && a.assignKind == DO.Hamal.inTreatment);

    //        if (volunteerActiveAssignment != null)
    //        {
    //            throw new Exception($"Volunteer with ID={volunteerId} is already working on another call.");
    //        }

    //        // חישוב המרחק ובדיקת טווח
    //        var distance = CalculateDistance(call.latitude ?? 0, call.longitude ?? 0, volunteer.latitude, volunteer.longitude);
    //        if (distance > volunteer.limitDestenation)
    //        {
    //            throw new Exception($"Call is out of volunteer's range (Distance: {distance} > Limit: {volunteer.limitDestenation}).");
    //        }

    //        // יצירת שיוך חדש
    //        var assignment = new DO.Assignment
    //        {
    //            callId = callId,
    //            volunteerId = volunteerId,
    //            startTime = AdminManager.Now,
    //            assignKind = DO.Hamal.inTreatment
    //        };
    //        _dal.assignment.Create(assignment);

    //        // עדכון סטטוס הקריאה
    //        var x = ConvertToBOCall(call);
    //        x.Status = Status.inProgres;
    //    }

    //    // עדכון התצפיתנים (מחוץ לנעילה)
    //    CallManager.Observers.NotifyListUpdated();

    //}
    public async Task AssignCallToVolunteer(int volunteerId, int callId)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7
        CallManager.AssignCallToVolunteer(volunteerId, callId);
    }


    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
       return CallManager.CalculateDistance(lat1, lon1, lat2, lon2);
    }

    

    public void CancelCallAssignment(int volunteerId, int callId, BO.Role role)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7
        CallManager.CancelCallAssignment(volunteerId,callId,role);
    }


    public void CloseCallAssignment(int volunteerId, int callId)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7
        CallManager.CloseCallAssignment(volunteerId,callId);
    }



    public void DeleteCall(int callId)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7

        lock (AdminManager.BlMutex) // נעילה סביב גישה ל-DAL
        {
            // בדיקת קיום הקריאה
            var call = _dal.call.Read(callId) ??
                throw new Exception($"Call with ID={callId} does not exist.");

            // שליפת כל ההקצאות הקשורות לקריאה
            var assignments = _dal.assignment.ReadAll(a => a.callId == callId);

            // בדיקה אם יש הקצאות כלשהן לקריאה
            if (assignments.Any())
            {
                throw new Exception($"Cannot delete a call with assignments (Call ID={callId}).");
            }

            try
            {
                // מחיקת הקריאה
                _dal.call.Delete(callId);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete call.", ex);
            }
        }

        // עדכון צופים (אם קיים מנגנון צופים)
        CallManager.Observers.NotifyListUpdated(); // שלב 5
    }

    public int[] GetCallCountsByStatus()
    {
        // טוען את כל הקריאות וההקצאות משכבת ה-DAL
        var calls = _dal.call.ReadAll();
        var assignments = _dal.assignment.ReadAll();

        // שליפת סטטוסים לכל קריאה
        var statuses = GetStatusesByCall(calls, assignments, TimeSpan.FromHours(1));

        // מערך לאחסון המספרים עבור כל סטטוס
        int[] statusCounts = new int[Enum.GetValues(typeof(Status)).Length];

        foreach (var status in statuses.Values)
        {
            statusCounts[(int)status]++;
        }

        return statusCounts;
    }

    public Call GetCallDetails(string calld)
    {
        var calls = _dal.call.ReadAll();
        int callId = 0;
        foreach (var call2 in calls)
        {
            if (call2.detail == calld)
            {
                callId = call2.id;
            }
        }
        // קריאת הקריאה הספציפית משכבת ה-DAL לפי המזהה
        var call = _dal.call.Read(int.Parse(calld));

        // בדיקת קיום הקריאה
        if (call == null)
        {
            throw new KeyNotFoundException($"Call with ID {callId} not found.");
        }

        // החזרת פרטי הקריאה
        return new BO.Call
        {
            Id = call.id,
            Description = call.detail,
            Address = call.adress,
            Latitude = call.latitude,
            Longitude = call.longitude,
            CallType = (BO.CallType)call.callType.GetValueOrDefault(),
            OpenTime = call.startTime ?? throw new InvalidOperationException("Start time is null."),
            MaxEndTime = call.maximumTime
        };
    }
    public IEnumerable<ClosedCallInList> GetClosedCallsByVolunteer(int vId, Enum? cType, Enum? sortField)
    {
        // קיבלתי את כל הקריאות שהוקצו למתנדב ויש להם זמן סיום
        var assigns = _dal.assignment.ReadAll()
            .Where(assign => assign.volunteerId == vId && assign.finishTime != null); // רק קריאות שסיימו טיפול

        var callIds = assigns.Select(assign => assign.callId);
        var calls = _dal.call.ReadAll(c => callIds.Contains(c.id)); // כל הקריאות המתאימות לפי מזהי הקריאות

        // מיזוג נתוני הקריאות והשיוכים ליצירת ClosedCallInList
        var closedCalls = from assign in assigns
                          join call in calls on assign.callId equals call.id
                          select new ClosedCallInList(
                              id: call.id,
                              callType: (CallType)(call.callType ?? 0), // המרת Enum
                              address: call.adress,
                              openTime: call.startTime ?? DateTime.MinValue,
                              assignmentStartTime: assign.startTime ?? DateTime.MinValue,
                              actualEndTime: assign.finishTime ?? null,
                              endType: (Hamal?)assign.assignKind ?? null// המרת Enum
                          );

        // סינון לפי סוג הקריאה אם צוין
        if (cType != null)
        {
            closedCalls = closedCalls.Where(c => c.CallType == (CallType)cType);
        }

        // מיון הקריאות לפי השדה הנבחר
        if (sortField != null)
        {
            closedCalls = sortField switch
            {
                CallField.Status => closedCalls.OrderBy(c => c.OpenTime), // מיון לפי זמן פתיחה
                CallField.AssignedTo => closedCalls.OrderBy(c => c.AssignmentStartTime), // מיון לפי זמן התחלה
                                                                                         //  CallField.Priority => closedCalls.OrderBy(c => c.ActualEndTime), // מיון לפי זמן סיום
                _ => closedCalls // ללא מיון אם השדה אינו נתמך
            };
        }

        return closedCalls;
    }

    public IEnumerable<OpenCallInList> GetOpenCallsByVolunteer(int volunteerId, CallType? callType = null, Enum? sortField = null)
    {
        return CallManager.GetOpenCallsByVolunteer(volunteerId, callType, sortField);
    }

    public Call? GetAssignedCallByVolunteer(int volunteerId)
    {
        // בדיקת מתנדב
        var volunteer = _dal.volunteer.Read(volunteerId);
        if (volunteer == null)
        {
            throw new KeyNotFoundException($"Volunteer with ID {volunteerId} not found.");
        }

        // חיפוש הקצאה פעילה למתנדב
        var assignment = _dal.assignment.ReadAll()
            .FirstOrDefault(a => a.volunteerId == volunteerId && a.assignKind == DO.Hamal.inTreatment);
        if (assignment == null)
        {
            return null;
        }

        // קריאת פרטי הקריאה
        var call = _dal.call.Read(assignment.callId);
        if (call == null)
        {
            throw new KeyNotFoundException($"Call with ID {assignment.callId} not found.");
        }

        // החזרת פרטי הקריאה
        return new BO.Call
        {
            Id = call.id,
            Description = call.detail,
            Address = call.adress,
            Latitude = call.latitude,
            Longitude = call.longitude,
            CallType = (BO.CallType)call.callType.GetValueOrDefault(),
            OpenTime = call.startTime ?? throw new InvalidOperationException("Start time is null."),
            MaxEndTime = call.maximumTime
        };
    }

    public async Task UpdateCallDetails(Call call)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7

        // וידוא שהקריאה אינה null
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call), "Call cannot be null.");
        }

        // בדיקת תקינות לוגית
        if (call.MaxEndTime <= call.OpenTime)
        {
            throw new ArgumentException("MaxEndTime must be greater than OpenTime.");
        }

        // עדכון קווי אורך ורוחב לפי הכתובת
        var coordinates = await VolunteerManager.GetCoordinatesFromGoogleAsync(call.Address);
        if (coordinates == null || coordinates.Length < 2)
        {
            throw new InvalidOperationException("Invalid address: could not retrieve coordinates.");
        }

        // נעילה סביב גישה ל-DAL
        lock (AdminManager.BlMutex) // שלב 7
        {
            // יצירת אובייקט DO.Call
            var doCall = new DO.Call
            {
                id = call.Id,
                detail = call.Description,
                adress = call.Address,
                latitude = coordinates[0],
                longitude = coordinates[1],
                callType = (DO.CallType?)call.CallType,
                startTime = call.OpenTime,
                maximumTime = call.MaxEndTime
            };

            // עדכון הקריאה בשכבת ה-DAL
            try
            {
                _dal.call.Update(doCall);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update call.", ex);
            }
        }

        // עדכון תצפיתנים (מחוץ לנעילה)
        CallManager.Observers.NotifyItemUpdated(call.Id); // שלב 5
        CallManager.Observers.NotifyListUpdated();        // שלב 5
    }


    public void UpdateCallStatus(Call call, bool isFinish)
    {
        if (isFinish)
            call.Status = Status.closed;
    }

    public IEnumerable<CallInList> GetCallsList(CallField? filterField, object? filterValue, CallField? sortField)
    {
        // טוען את כל הקריאות וההקצאות משכבת ה-DAL
        var calls = _dal.call.ReadAll();
        var assignments = _dal.assignment.ReadAll();

        // שליפת סטטוסים לכל קריאה
        var statuses = GetStatusesByCall(calls, assignments, TimeSpan.FromHours(1));

        // מציאת ההקצאה האחרונה לכל קריאה לפי ה-Id של ההקצאה
        var latestAssignments = assignments
            .GroupBy(a => a.callId) // קיבוץ ההקצאות לפי מזהה הקריאה
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.id).FirstOrDefault()
            );

        AdminImplementation admin = new();

        // מיזוג נתוני הקריאות עם ההקצאות
        var callAssignments = from call in calls
                              let assign = latestAssignments.TryGetValue(call.id, out var latestAssign) ? latestAssign : null
                              select new CallInList
                              {
                                  CallId = call.id,
                                  CallType = (CallType)(call.callType ?? 0),
                                  OpenTime = call.startTime ?? DateTime.MinValue,
                                  TimeRemaining = call.maximumTime.HasValue
                                      ? call.maximumTime.Value - DateTime.Now
                                      : (TimeSpan?)null,
                                  LastVolunteerName = assign?.volunteerId != null &&
                                                      (assign.assignKind != DO.Hamal.cancelByManager && assign.assignKind != DO.Hamal.cancelByVolunteer)
                                      ? _dal.volunteer.Read(assign.volunteerId)?.name
                                      : null,
                                  CompletionTime = assign?.finishTime != null
                                      ? assign.finishTime.Value - (call.startTime ?? DateTime.MinValue)
                                      : null,
                                  TotalAssignments = assignments.Count(a => a.callId == call.id),
                                  Status = statuses[call.id]
                              };

        // סינון הקריאות לפי שדה וערך (אם נבחרו)
        if (filterField != null)
        {
            switch (filterField)
            {
                case CallField.Status:
                    {
                        //if (filterValue is object Status)
                        //{
                        //    callAssignments = callAssignments.Where(c => c.Status == BO.Status.open);
                        //}
                        if (filterValue is BO.Status statusFilter)
                        {
                            callAssignments = callAssignments.Where(c => c.Status == statusFilter);
                        }
                            break;
                    }

                case CallField.AssignedTo:
                    if (filterValue is string assignedTo)
                    {
                        callAssignments = callAssignments.Where(c => c.LastVolunteerName != null);
                    }
                    break;
                

                // הוסף סינונים נוספים אם יש צורך
                default:
                    break;
            }
        }

        // מיון הקריאות לפי שדה שנבחר
        if (sortField != null)
        {
            switch (sortField)
            {
                case CallField.Status:
                    callAssignments = callAssignments.OrderBy(c => c.Status);
                    break;

                case CallField.AssignedTo:
                    callAssignments = callAssignments.OrderBy(c => c.LastVolunteerName);
                    break;

                default:
                    callAssignments = callAssignments.OrderBy(c => c.CallId);
                    break;
            }
        }
        else
        {
            callAssignments = callAssignments.OrderBy(c => c.CallId);
        }

        return callAssignments;
    }


    public Status UpdateStatus(Call call, TimeSpan riskTime)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        if (call.MaxEndTime.HasValue)
        {
            var timeRemaining = call.MaxEndTime.Value - DateTime.Now;

            // אם הזמן של הקריאה עבר (timeRemaining < 0), החזיר סטטוס "פג תוקף"
            if (timeRemaining <= TimeSpan.Zero)
            {
                return Status.expired;
            }

            // אם הקריאה פתוחה ולא בטיפול
            if (call.Status == Status.open)
            {
                // קריאה פתוחה בסיכון
                if (timeRemaining <= riskTime)
                {
                    return Status.openInRisk;
                }
                // קריאה פתוחה רגילה
                return Status.open;
            }

            // אם הקריאה בטיפול
            if (call.Status == Status.inProgres)
            {
                // קריאה בטיפול בסיכון
                if (timeRemaining <= riskTime)
                {
                    return Status.openInRisk;
                }
                // קריאה בטיפול רגילה
                return Status.inProgres;
            }

            // אם הקריאה סיימה טיפול (סגורה)
            if (call.Status == Status.closed)
            {
                return Status.closed;
            }
        }
        CallManager.Observers.NotifyListUpdated(); // שלב 5
        // אם לא נמצא סטטוס מתאים, נשאיר את הסטטוס הקיים
        return call.Status;
    }



    public BO.Call ConvertToBOCall(DO.Call doCall)
    {
        return CallManager.ConvertToBOCall(doCall);
    }


    private Status DetermineStatus(DO.Call call, DO.Assignment? assign, TimeSpan riskTimeSpan, DateTime systemTime)
    {
        var adminImplementation = new AdminImplementation();
        var systemClock = adminImplementation.GetSystemClock();

        // אם הזמן עבר ואין הקצאה
        if (assign == null && call.maximumTime < systemClock)
        {
            return Status.expired;
        }

        // אם הזמן עבר ויש הקצאה פעילה
        if (assign != null && call.maximumTime < systemClock && assign.finishTime == null)
        {
            return Status.expired;
        }

        // אם הזמן עבר ויש הקצאה שהסתיימה
        if (assign != null && call.maximumTime < systemClock && assign.finishTime.HasValue)
        {
            return call.maximumTime.HasValue && assign.finishTime.Value > call.maximumTime - riskTimeSpan
                ? Status.closeInRisk
                : Status.closed;
        }

        // קריאה ללא הקצאה והזמן לא עבר
        if (assign == null || assign.assignKind == DO.Hamal.cancelByManager || assign.assignKind == DO.Hamal.cancelByVolunteer)
        {
            return call.maximumTime < systemClock + riskTimeSpan ? Status.openInRisk : Status.open;
        }

        // קריאה עם הקצאה פעילה והזמן לא עבר
        if (assign.finishTime == null)
        {
            return call.maximumTime < systemClock + riskTimeSpan ? Status.openInRisk : Status.inProgres;
        }

        // קריאה שהסתיימה והזמן לא עבר
        if (assign.finishTime.HasValue)
        {
            return call.maximumTime.HasValue && assign.finishTime.Value > call.maximumTime - riskTimeSpan
                ? Status.closeInRisk
                : Status.closed;
        }
        CallManager.Observers.NotifyListUpdated();

        // ברירת מחדל (לא אמור לקרות)
        throw new InvalidOperationException("Unable to determine status for the given input.");
    
}
    //private Dictionary<int, Status> GetStatusesByCall(IEnumerable<DO.Call> calls, IEnumerable<DO.Assignment> assignments, TimeSpan riskTimeSpan)
    //{
    //    var latestAssignments = assignments
    //        .GroupBy(a => a.callId)
    //        .ToDictionary(
    //            g => g.Key,
    //            g => g.OrderByDescending(a => a.id).FirstOrDefault()
    //        );


    //    return calls.ToDictionary(
    //            call => call.id,
    //            call =>
    //            {
    //                var assignment = latestAssignments.TryGetValue(call.id, out var assign) ? assign : null;
    //                var currentSystemTime = DateTime.Now;
    //                return DetermineStatus(call, assign, riskTimeSpan, currentSystemTime);
    //            }
    //        );
    //}
    private Dictionary<int, Status> GetStatusesByCall(IEnumerable<DO.Call> calls, IEnumerable<DO.Assignment> assignments, TimeSpan riskTimeSpan)
    {
        // זיהוי כפילויות ב-calls
        var duplicateCallIds = calls.GroupBy(c => c.id)
                                    .Where(g => g.Count() > 1)
                                    .Select(g => g.Key);

        if (duplicateCallIds.Any())
        {
            throw new Exception($"Duplicate call IDs found: {string.Join(", ", duplicateCallIds)}");
        }

        // זיהוי כפילויות ב-assignments
        var duplicateAssignmentIds = assignments.GroupBy(a => a.callId)
                                                .Where(g => g.Count() > 1)
                                                .Select(g => g.Key);

        if (duplicateAssignmentIds.Any())
        {
            throw new Exception($"Duplicate assignment IDs found: {string.Join(", ", duplicateAssignmentIds)}");
        }

        // יצירת מילון של ההשמות האחרונות
        var latestAssignments = assignments
            .GroupBy(a => a.callId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.id).FirstOrDefault()
            );

        // יצירת מילון של הסטטוסים
        return calls.ToDictionary(
            call => call.id,
            call =>
            {
                var assignment = latestAssignments.TryGetValue(call.id, out var assign) ? assign : null;
                var currentSystemTime = DateTime.Now;
                return DetermineStatus(call, assignment, riskTimeSpan, currentSystemTime);
            }
        );
    }

    public IEnumerable<CallAssignInList> GetAssignmentsForCall(int callId)
    {
        // שליפת הקריאה
        var call = _dal.call.Read(callId) ??
            throw new Exception($"Call with ID={callId} does not exist.");

        // שליפת כל ההשמות הקשורות לקריאה
        var assignments = _dal.assignment.ReadAll()
            .Where(a => a.callId == callId)
            .ToList();

        if (!assignments.Any())
        {
            throw new Exception($"No assignments found for Call ID={callId}.");
        }

        // מיפוי ההשמות למודל BO
        var assignmentList = assignments.Select(assign =>
        {
            var volunteer = _dal.volunteer.Read(assign.volunteerId);

            return new CallAssignInList
            {
                VolunteerId = assign.volunteerId,
                VolunteerName = volunteer?.name,
                EntryTime = assign.startTime ?? DateTime.MinValue,
                EndTime = assign.finishTime,
                EndType = (BO.Hamal)assign.assignKind
            };
        });

        return assignmentList;
    }
    public CallInProgress? GetActiveAssignmentForVolunteer(int volunteerId)
    {
        // שליפת מתנדב
        var volunteer = _dal.volunteer.Read(volunteerId) ??
            throw new Exception($"Volunteer with ID={volunteerId} does not exist.");

        if (volunteer.latitude == null || volunteer.longitude == null)
        {
            throw new Exception($"Location data for Volunteer ID={volunteerId} is not provided.");
        }

        // שליפת השיוך הפעיל של המתנדב
        var activeAssignment = _dal.assignment.ReadAll()
            .FirstOrDefault(a => a.volunteerId == volunteerId && a.finishTime == null && a.assignKind == DO.Hamal.inTreatment);

        if (activeAssignment == null)
        {
            return null; // אין שיוך פעיל
        }

        // שליפת הקריאה הקשורה לשיוך הפעיל
        var call = _dal.call.Read(activeAssignment.callId) ??
            throw new Exception($"Call with ID={activeAssignment.callId} does not exist.");

        // יצירת האובייקט CallInProgress
        return new CallInProgress
        {
            Id = activeAssignment.id,
            CallId = call.id,
            CallType = (CallType)(call.callType ?? 0),
            Description = call.detail,
            FullAddress = call.adress,
            OpenTime = call.startTime ?? DateTime.MinValue,
            MaxCloseTime = call.maximumTime,
            EntryTime = activeAssignment.startTime ?? DateTime.MinValue,
            DistanceFromVolunteer = CalculateDistance(call.latitude ?? 0, call.longitude ?? 0, volunteer.latitude, volunteer.longitude),
            Status = Status.inProgres // השיוך פעיל, ולכן הסטטוס הוא "בטיפול"
        };
    }
   
        public void AddObserver(Action listObserver) =>
    CallManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
    CallManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
    CallManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
    CallManager.Observers.RemoveObserver(id, observer); //stage 5
}