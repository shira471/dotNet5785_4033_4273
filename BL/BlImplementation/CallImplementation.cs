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
using DO;


public class CallImplementation : ICall
{
    private static readonly DalApi.Idal _dal = DalApi.Factory.Get;

    public void AddCall(BO.Call call)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7

        if (call == null)
            throw new ArgumentNullException(nameof(call), "Call cannot be null.");

        if (call.CallType == BO.CallType.None)
            throw new ArgumentException("Call type cannot be None.");

        if (call.MaxEndTime <= call.OpenTime)
            throw new ArgumentException("End time cannot be earlier than start time.");

        if (call.MaxEndTime == null)
            throw new ArgumentException("End time cannot be null.");

        // ✔ קריאה אסינכרונית לקבלת ה-ID הגבוה ביותר
        int maxId = 0;
        var calls =  _dal.call.ReadAll(); // נדרש await במקרה שזה async
        if (calls != null && calls.Any())
        {
            maxId = calls.Max(c => c.id) + 1; // מחזירה את ה-ID הגבוה ביותר ומוסיפה 1
        }

        // ✔ שליחה מיידית ל-DAL *בלי* הקואורדינטות
        var doCall = new DO.Call(
            id: maxId,
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
        CallManager.Observers.NotifyListUpdated();

        // ✔ חישוב קואורדינטות ברקע בלי לחכות
        _ = CallManager.UpdateCallCoordinatesAsync(doCall);
    }

    
    public void  AssignCallToVolunteer(int volunteerId, int callId)
    {
        
            AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7 
        try
        {
            CallManager.AssignCallToVolunteer(volunteerId, callId);
        }
        catch (Exception ex)
        {
            throw;
        }


    }


    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        try
        {
            return CallManager.CalculateDistance(lat1, lon1, lat2, lon2);
        }catch(Exception ex)
        {
            throw;
        }
    }

    

    public void CancelCallAssignment(int volunteerId, int callId, BO.Role role)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7
        try
        {
            CallManager.CancelCallAssignment(volunteerId, callId, role);
        }catch(Exception ex)
        {
            throw;
        }
    }


    public void CloseCallAssignment(int volunteerId, int callId)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7
        try
        {
            CallManager.CloseCallAssignment(volunteerId, callId);
        }catch(Exception ex)
        {
            throw;
        }
    }



    public void DeleteCall(int callId)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7
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
            lock (AdminManager.BlMutex) // נעילה סביב גישה ל-DAL
            {
                // מחיקת הקריאה
                _dal.call.Delete(callId);

            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to delete call.", ex);
        }
        

        // עדכון צופים (אם קיים מנגנון צופים)
        CallManager.Observers.NotifyListUpdated(); // שלב 5
    }

    public int[] GetCallCountsByStatus()
    {
        List<DO.Call> calls;
        List<DO.Assignment> assignments;
        TimeSpan risk;

        // נעילה אחת לכל קריאות ה-DAL כדי למנוע קריסות
        lock (AdminManager.BlMutex)
        {
            calls = _dal.call.ReadAll().ToList(); // קריאת כל הקריאות
            assignments = _dal.assignment.ReadAll().ToList(); // קריאת כל השיוכים
            risk = new AdminImplementation().GetRiskTimeSpan(); // שליפת זמן סיכון
        }

        // שליפת סטטוסים לכל קריאה
        var statuses = GetStatusesByCall(calls, assignments, risk);

        // מערך לאחסון המספרים עבור כל סטטוס
        int[] statusCounts = new int[Enum.GetValues(typeof(Status)).Length];

        foreach (var status in statuses.Values)
        {
            statusCounts[(int)status]++;
        }
        return statusCounts;
    }

    public BO.Call GetCallDetails(string calld)
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
                              callType: (BO.CallType)(call.callType ?? 0), // המרת Enum
                              address: call.adress,
                              openTime: call.startTime ?? DateTime.MinValue,
                              assignmentStartTime: assign.startTime ?? DateTime.MinValue,
                              actualEndTime: assign.finishTime ?? null,
                              endType: (BO.Hamal?)assign.assignKind ?? null// המרת Enum
                          );

        // סינון לפי סוג הקריאה אם צוין
        if (cType != null)
        {
            closedCalls = closedCalls.Where(c => c.CallType == (BO.CallType)cType);
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

    public IEnumerable<OpenCallInList> GetOpenCallsByVolunteer(int volunteerId, BO.CallType? callType = null, Enum? sortField = null)
    {
        try
        {
            return CallManager.GetOpenCallsByVolunteer(volunteerId, callType, sortField);
        }catch (Exception e)
        {
            throw;
        }
    }

    public BO.Call? GetAssignedCallByVolunteer(int volunteerId)
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

    public void UpdateCallDetails(BO.Call call)
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // שלב 7
        try
        {
            CallManager.UpdateCallDetails(call);
        }catch (Exception e)
        {
            throw;
        }
    }


    public void UpdateCallStatus(BO.Call call, bool isFinish)
    {
        if (isFinish)
            call.Status = Status.closed;
    }

    public IEnumerable<CallInList> GetCallsList(CallField? filterField, object? filterValue, CallField? sortField)
    {
        // טוען את כל הקריאות וההקצאות משכבת ה-DAL
        var calls = _dal.call.ReadAll();
        var assignments = _dal.assignment.ReadAll();
        var risk = new AdminImplementation().GetRiskTimeSpan();
        // שליפת סטטוסים לכל קריאה
        var statuses = GetStatusesByCall(calls, assignments, risk);

        // מציאת ההקצאה האחרונה לכל קריאה לפי ה-Id של ההקצאה
        var latestAssignments = assignments
            .GroupBy(a => a.callId) // קיבוץ ההקצאות לפי מזהה הקריאה
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.id).FirstOrDefault()
            );

        AdminImplementation admin = new();
        var adminImplementation = new AdminImplementation();
        var systemClock = adminImplementation.GetSystemClock();
        // מיזוג נתוני הקריאות עם ההקצאות
        var callAssignments = from call in calls
                              let assign = latestAssignments.TryGetValue(call.id, out var latestAssign) ? latestAssign : null
                              select new CallInList
                              {
                                  CallId = call.id,
                                  CallType = (BO.CallType)(call.callType ?? 0),
                                  OpenTime = call.startTime ?? DateTime.MinValue,
                                  TimeRemaining = call.maximumTime.HasValue ? new TimeSpan(
                                                                                             (call.maximumTime.Value - systemClock).Days,
                                                                                             (call.maximumTime.Value - systemClock).Hours,
                                                                                             (call.maximumTime.Value - systemClock).Minutes, 0)
                                                                                                : (TimeSpan?)null,
                                  LastVolunteerName = assign?.volunteerId != null &&
                                                      (assign.assignKind != DO.Hamal.cancelByManager && assign.assignKind != DO.Hamal.cancelByVolunteer)
                                      ? _dal.volunteer.Read(assign.volunteerId)?.name
                                      : null,
                                  CompletionTime = assign?.finishTime != null
                                      ? assign.finishTime.Value - (call.startTime ?? systemClock)
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
                            if(BO.Status.None== statusFilter)
                            {
                                break;
                            }
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


    public Status UpdateStatus(BO.Call call, TimeSpan riskTime)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        var adminImplementation = new AdminImplementation();
        var systemClock = adminImplementation.GetSystemClock();
        if (call.MaxEndTime.HasValue)
        {
            var timeRemaining = call.MaxEndTime.Value - systemClock;

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
            var updatedAssignment = assign with { assignKind = DO.Hamal.handelExpired };

            lock (AdminManager.BlMutex)
            {
                _dal.assignment.Update(updatedAssignment);
            }
            return Status.expired;
        }

        // אם הזמן עבר ויש הקצאה פעילה
        if (assign != null && call.maximumTime < systemClock && assign.finishTime == null)
        {
            var updatedAssignment = assign with { assignKind = DO.Hamal.handelExpired };

            lock (AdminManager.BlMutex)
            {
                _dal.assignment.Update(updatedAssignment);
            }
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

    private Dictionary<int, Status> GetStatusesByCall(IEnumerable<DO.Call> calls, IEnumerable<DO.Assignment> assignments, TimeSpan riskTimeSpan)
    {
        // זיהוי כפילויות ב-calls
        //var duplicateCallIds = calls.GroupBy(c => c.id)
        //                            .Where(g => g.Count() > 1)
        //                            .Select(g => g.Key);

        //if (duplicateCallIds.Any())
        //{
        //    throw new Exception($"Duplicate call IDs found: {string.Join(", ", duplicateCallIds)}");
        //}

        // זיהוי כפילויות ב-assignments
        //var duplicateAssignmentIds = assignments.GroupBy(a => a.callId)
        //                                        .Where(g => g.Count() > 1)
        //                                        .Select(g => g.Key);

        // יצירת מילון של ההשמות האחרונות
        var latestAssignments = assignments
            .GroupBy(a => a.callId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.id).FirstOrDefault()
            );
        var adminImplementation = new AdminImplementation();
        var systemClock = adminImplementation.GetSystemClock();
        // יצירת מילון של הסטטוסים
        return calls.ToDictionary(
            call => call.id,
            call =>
            {
                var assignment = latestAssignments.TryGetValue(call.id, out var assign) ? assign : null;
                var currentSystemTime = systemClock;
                return DetermineStatus(call, assignment, riskTimeSpan, currentSystemTime);
            }
        );
    }

    public IEnumerable<CallAssignInList> GetAssignmentsForCall(int callId)
    {
        try { 
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
        catch (NullReferenceException ex)
        {
            // טיפול בחריגות NullReferenceException
            Console.WriteLine($"Error: A null reference occurred. Details: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            // טיפול בחריגות InvalidOperationException
            Console.WriteLine($"Error: An invalid operation occurred. Details: {ex.Message}");
        }
        catch (Exception ex)
        {
            // טיפול בכל החריגות האחרות
            Console.WriteLine($"Error: {ex.Message}");
        }

        // במקרה של חריגה, להחזיר רשימה ריקה
        return Enumerable.Empty<CallAssignInList>();
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
            CallType = (BO.CallType)(call.callType ?? 0),
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