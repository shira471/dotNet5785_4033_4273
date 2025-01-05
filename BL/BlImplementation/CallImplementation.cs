namespace BlImplementation;

using System;
using System.Collections.Generic;
using BL.Helpers;
using BlApi;
using BO;
using DalApi;

using Helpers;


public class CallImplementation : ICall
{
    private readonly DalApi.Idal _dal = DalApi.Factory.Get;

    public void AddCall(Call call)
    {
        var calls = _dal.call.ReadAll();
        foreach (var call2 in calls)
        {
            if (call2.detail == call.Description)
            {
                throw new Exception("this call is already exist");
            }
        }
        if (call == null)
        {
            throw new ArgumentNullException("Call cannot be null.");
        }

        try
        {
            var temp = VolunteerManager.GetCoordinatesFromGoogle(call.Address);
            var doCall = new DO.Call(
                id: _dal.config.getNextCallId(), // ייווצר מזהה חדש ב-DAL
            detail: call.Description,
            adress: call.Address,
            latitude: temp[0],
            longitude: temp[1],
            callType: (DO.Hamal?)call.CallType,
            startTime: call.OpenTime,
            maximumTime: call.MaxEndTime
                );
            _dal.call.Create(doCall);
            call.Status = Status.Open;
            CallManager.Observers.NotifyListUpdated(); //stage 5
         }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new Exception(ExceptionsManager.HandleException(new Exception("Failed to add call.")));
        }
    }


    public void AssignCallToVolunteer(int volunteerId, string calldes)
    {
        //שליפת הקריאה ממאגר הנתונים
        var calls = _dal.call.ReadAll();
        int callId=0;
        foreach (var call2 in calls)
        {
            if (call2.detail == calldes)
            {
                callId = call2.id;
            }
        }
        var call = _dal.call.Read(callId) ??
            throw new Exception($"Call with ID={callId} does not exist.");
        //שליפת המתנדב ממאגר הנתונים
        var volunteer = _dal.volunteer.Read(volunteerId) ??
           throw new Exception($"Volunterr with ID={volunteerId} does not exist.");
        var dis = CalculateDistance(call.latitude ?? 0, call.longitude ?? 0, volunteer.latitude, volunteer.longitude);
        if (dis > volunteer.limitDestenation)
        {
            throw new Exception($"Call is out of volunteer's range (Distance: {dis} > Limit: {volunteer.limitDestenation}).");
        }

        // צור שיוך חדש
        var assignment = new DO.Assignment
        {
            callId = callId,
            volunteerId = volunteerId,
            startTime = AdminManager.Now
        };
        _dal.assignment.Create(assignment);
        CallManager.Observers.NotifyListUpdated(); //stage 5
    }
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
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

    private double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);

    public void CancelCallAssignment(int requesterId, int assignmentId)
    {
        // בדיקת השיוך
        var assign = _dal.assignment.Read(assignmentId) ??
            throw new Exception($"Assignment with ID={assignmentId} does not exist.");

        // בדיקת בקשה נכונה
        var call = _dal.call.Read(assign.callId) ??
            throw new Exception($"Call with ID={assign.callId} does not exist.");


        // עדכון בסיס הנתונים
        _dal.assignment.Delete(assignmentId);
        CallManager.Observers.NotifyListUpdated(); //stage 5
    }

    public void CloseCallAssignment(int volunteerId, int assignmentId)
    {
        // בדיקת קיום השיוך
        var assign = _dal.assignment.Read(assignmentId) ??
            throw new Exception($"Assignment with ID={assignmentId} does not exist.");
        // בדיקת התאמה למתנדב
        if (assign.volunteerId != volunteerId)
        {
            throw new UnauthorizedAccessException("Volunteer does not have permission to close this assignment.");
        }
        // בדיקת סטטוס השיוך
        if (assign.assignKind != null)
        {
            throw new Exception($"Assignment with ID={assignmentId} has already been closed.");
        }
        // עדכון זמן סיום השיוך
        var updatedAssignment = assign with
        {
            finishTime = DateTime.Now
        };
        _dal.assignment.Update(updatedAssignment);

        // עדכון סטטוס הקריאה
        var call = _dal.call.Read(assign.callId) ??
            throw new Exception($"Call with ID={assign.callId} does not exist.");

        var updatedCall = call with { callType = null };
        _dal.call.Update(updatedCall);

    }

    public void DeleteCall(int callId)
    {
        // בדיקת קיום הקריאה
        var call = _dal.call.Read(callId) ??
            throw new Exception($"Call with ID={callId} does not exist.");

        // בדיקת סטטוס הקריאה
        if (call.callType == null)
        {
            throw new Exception($"Cannot delete a completed call with ID={callId}.");
        }

        // בדיקת שיוכים פעילים לקריאה
        var assignments = _dal.assignment.ReadAll(a => a.callId == callId);
        if (assignments.Any(a => a.assignKind == null))
        {
            throw new Exception($"Cannot delete a call with active assignments (Call ID={callId}).");
        }

        // מחיקת כל השיוכים הקשורים לקריאה
        foreach (var assignment in assignments)
        {
            _dal.assignment.Delete(assignment.id);
        }

        // מחיקת הקריאה עצמה
        _dal.call.Delete(callId);
        CallManager.Observers.NotifyListUpdated(); //stage 5
    }
    public int[] GetCallCountsByStatus()
    {

        // רשימת כל הקריאות
        var calls = _dal.call.ReadAll(); // קריאה לשכבת ה-DAL לקבלת הקריאות

        // מערך לאחסון המספרים עבור כל סטטוס
        int[] statusCounts = new int[Enum.GetValues(typeof(Status)).Length];

        // מעבר על כל הקריאות וספירה לפי סטטוס
        foreach (var call in calls)
        {
            Status status;

            // הגדרת סטטוס מבוסס על הנתונים הקיימים
            if (call.maximumTime.HasValue && call.maximumTime <= DateTime.Now)
            {
                // אם זמן המקסימום חלף, הקריאה נסגרת
                status = Status.Closed;
            }
            //else if (call.startTime.HasValue)
            //{
            //    // אם יש זמן התחלה אך עדיין לא הסתיימה
            //    status = Status.InProgress;
            //}
            else
            {
                // אחרת, הקריאה ממתינה לטיפול
                status = Status.Open;
            }

            // הגדלת הספירה עבור הסטטוס המתאים
            statusCounts[(int)status]++;
        }

        return statusCounts; // החזרת המערך
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
        var call = _dal.call.Read(callId);

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
                              actualEndTime: assign.finishTime,
                              endType: (EndType?)(assign.assignKind ?? null) // המרת Enum
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
                CallField.Priority => closedCalls.OrderBy(c => c.ActualEndTime), // מיון לפי זמן סיום
                _ => closedCalls // ללא מיון אם השדה אינו נתמך
            };
        }

        return closedCalls;
    }

    public IEnumerable<OpenCallInList> GetOpenCallsByVolunteer(int volunteerId, Enum? callType=null, Enum? sortField = null)
    {
        // קבלת כל השיוכים של המתנדב לקריאות שטרם נסגרו
        var assignments = _dal.assignment.ReadAll()
            .Where(assign => assign.volunteerId == volunteerId && assign.finishTime == null); // קריאות פתוחות

        var callIds = assignments.Select(assign => assign.callId);
        var calls = _dal.call.ReadAll(c => callIds.Contains(c.id)); // קריאות פתוחות המתאימות למתנדב

        // מציאת המתנדב (בהנחה שמאגר המתנדבים נקרא _dal.volunteer)
        var volunteer = _dal.volunteer.ReadAll().FirstOrDefault(v => v.idVol == volunteerId);
        if (volunteer == null)
        {
            throw new InvalidOperationException("המתנדב לא נמצא");
        }
        // מיזוג נתוני הקריאות והשיוכים ליצירת OpenCallInList
        var openCalls = from assign in assignments
                        join call in calls on assign.callId equals call.id
                        select new OpenCallInList
                        {
                            Id = call.id,
                            Tkoc = (TheKindOfCall)(call.callType ?? 0), // המרת Enum עבור סוג הקריאה
                            Description = call.detail,
                            Address = call.adress,
                            OpenTime = call.startTime ?? DateTime.MinValue,
                            MaxEndTime = call.maximumTime,
                            DistanceFromVolunteer = CalculateDistance(call.latitude ?? 0, call.longitude ?? 0, volunteer.latitude, volunteer.longitude)
                        };

        // סינון לפי סוג הקריאה אם צוין
        if (callType != null)
        {
            openCalls = openCalls.Where(c => c.Tkoc == (TheKindOfCall)callType);
        }

        // מיון הקריאות לפי השדה הנבחר
        if (sortField != null)
        {
            openCalls = sortField switch
            {
                CallField.Status => openCalls.OrderBy(c => c.OpenTime),
                CallField.AssignedTo => openCalls.OrderBy(c => c.MaxEndTime),
                _ => openCalls // ללא מיון אם השדה אינו נתמך
            };
        }

        return openCalls;
    }


    public void UpdateCallDetails(Call call)
    {
        // וודא שהקריאה אינה null
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call), "Call cannot be null.");
        }

        // עדכון פרטי הקריאה במסד הנתונים
        // נניח שיש לך פונקציה ב-DAL שתעדכן את הקריאה
        var doCall = new DO.Call(//הפיכה מאובייקט מסוג BO לDO
               id: 0, // ייווצר מזהה חדש ב-DAL
           detail: call.Description,
           adress: call.Address,
           latitude: call.Latitude,
           longitude: call.Longitude,
           callType: (DO.Hamal?)call.CallType,
           startTime: call.OpenTime,
           maximumTime: call.MaxEndTime
               );
        _dal.call.Update(doCall);
        CallManager.Observers.NotifyItemUpdated(doCall.id); //stage 5
        CallManager.Observers.NotifyListUpdated(); //stage 5

    }

    public void UpdateCallStatus(Call call,bool isFinish)
    {
        if (isFinish) 
            call.Status = Status.Closed;
    }
    public IEnumerable<CallInList> GetCallsList(Enum? filterField, object? filterValue, Enum? sortField)
    {
        // קבלת כל הקריאות
        var calls = _dal.call.ReadAll(); // או כל קריאה ל- DAL כדי להביא את כל הקריאות
        var assigns = _dal.assignment.ReadAll(); // כל ההקצאות

        // מיזוג הקריאות וההקצאות על פי ה-callId
        var callAssignments = from call in calls
                              join assign in assigns on call.id equals assign.callId
                              select new CallInList
                              {
                                  CallId = call.id,
                                  CallType = (CallType)(call.callType ?? 0),
                                  OpenTime = call.startTime ?? DateTime.MinValue,
                                  TimeRemaining = call.maximumTime.HasValue ? call.maximumTime.Value - DateTime.Now : (TimeSpan?)null,
                                  LastVolunteerName = _dal.volunteer.Read(assign.volunteerId)?.name,
                                  CompletionTime = (call.maximumTime.HasValue && assign.finishTime.HasValue) ? (assign.finishTime.Value - call.startTime.Value) : null,
                                  Status = Status.Open, // סטטוס כרגע הוא פתוח, תוכל לעדכן בהתאם למצב הסופי
                                  TotalAssignments = assigns.Count(a => a.callId == call.id) // סך ההקצאות לכל קריאה
                              };

        // סינון לפי השדה הנבחר אם יש
        if (filterField != null && filterValue != null)
        {
            switch (filterField)
            {
                case CallField.Status:
                    var statusFilter = (Status)filterValue;
                    callAssignments = callAssignments.Where(c => c.Status == statusFilter);
                    break;
                case CallField.AssignedTo:
                    var volunteerName = (string)filterValue;
                    callAssignments = callAssignments.Where(c => c.LastVolunteerName == volunteerName);
                    break;
                case CallField.Priority:
                    // סינון לפי עדיפות - להוסיף אם יש לך מנגנון לזה
                    break;
                default:
                    break;
            }
        }

        // מיון לפי השדה שנבחר אם יש
        if (sortField != null)
        {
            switch (sortField)
            {
                case CallField.Status:
                    callAssignments = callAssignments.OrderBy(c => c.OpenTime);
                    break;
                case CallField.AssignedTo:
                    callAssignments = callAssignments.OrderBy(c => c.LastVolunteerName);
                    break;
                case CallField.Priority:
                    // מיון לפי עדיפות אם יש
                    break;
                default:
                    break;
            }
        }

        return callAssignments;
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