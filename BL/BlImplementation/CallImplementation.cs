namespace BlImplementation;

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Metadata;
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

        if (call.MaxEndTime < call.OpenTime)
        {
            throw new ArgumentException("End time cannot be earlier than start time.");
        }
        try
        {
            var temp = VolunteerManager.GetCoordinatesFromGoogle(call.Address);
            var doCall = new DO.Call(
                id: 0, // ייווצר מזהה חדש ב-DAL
            detail: call.Description,
            adress: call.Address,
            latitude: temp[0],
            longitude: temp[1],
            callType: (DO.CallType?)call.CallType,
            startTime: call.OpenTime,
            maximumTime: call.MaxEndTime
                );
            _dal.call.Create(doCall);
            call.Status = Status.open;
            AdminImplementation admin = new AdminImplementation();
            UpdateStatus(call, admin.GetRiskTimeSpan());
            CallManager.Observers.NotifyListUpdated(); //stage 5
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new Exception(ExceptionsManager.HandleException(new Exception("Failed to add call.")));
        }
    }


    public void AssignCallToVolunteer(int volunteerId, int callId)
    {
        // שליפת הקריאה ממאגר הנתונים
        var call = _dal.call.Read(callId) ??
            throw new Exception($"Call with ID={callId} does not exist.");

        // שליפת המתנדב ממאגר הנתונים
        var volunteer = _dal.volunteer.Read(volunteerId) ??
            throw new Exception($"Volunteer with ID={volunteerId} does not exist.");

        // חישוב המרחק ובדיקת טווח
        var distance = CalculateDistance(call.latitude ?? 0, call.longitude ?? 0, volunteer.latitude, volunteer.longitude);
        if (distance > volunteer.limitDestenation)
        {
            throw new Exception($"Call is out of volunteer's range (Distance: {distance} > Limit: {volunteer.limitDestenation}).");
        }

        // יצירת שיוך חדש
        var assignment = new DO.Assignment
        {
            callId = callId,
            volunteerId = volunteerId,
            startTime = AdminManager.Now,
            assignKind = DO.Hamal.handeled
        };
        _dal.assignment.Create(assignment);
        var x = ConvertToBOCall(call);
        x.Status = Status.inProgres;
        
        // עדכון התצפיתנים
        CallManager.Observers.NotifyListUpdated();
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

    public void CancelCallAssignment(int volunteerId, int callId)
    {
        // בדיקת השיוך
        var assignments = _dal.assignment.ReadAll()
            .Where(a => a.volunteerId == volunteerId && a.callId == callId)
            .ToList();

        if (!assignments.Any())
        {
            throw new Exception($"No assignment found for Volunteer ID={volunteerId} and Call ID={callId}.");
        }

        if (assignments.Count > 1)
        {
            throw new Exception($"Multiple assignments found for Volunteer ID={volunteerId} and Call ID={callId}.");
        }

        var assign = assignments.First();

        // בדיקת הקריאה
        var call = _dal.call.Read(callId) ??
            throw new Exception($"Call with ID={callId} does not exist.");
        // יצירת שיוך חדש
      

        _dal.assignment.Update(new DO.Assignment
        {
            id = assign.id, // הוספת מזהה ה-assignment
            callId = assign.callId,
            volunteerId = assign.volunteerId,
            startTime = assign.startTime,
            assignKind = assign.assignKind
        });
       
        // עדכון צופים
        CallManager.Observers.NotifyListUpdated(); // Stage 5
    }

    public void CloseCallAssignment(int volunteerId, int callId)
    {
        // חיפוש השיוך לפי מתנדב וקריאה
        var assignments = _dal.assignment.ReadAll()
            .Where(a => a.volunteerId == volunteerId && a.callId == callId)
            .ToList();

        if (!assignments.Any())
        {
            throw new Exception($"No assignment found for Volunteer ID={volunteerId} and Call ID={callId}.");
        }

        // בדיקת שיוכים מרובים
        if (assignments.Count > 1)
        {
            throw new Exception($"Multiple assignments found for Volunteer ID={volunteerId} and Call ID={callId}.");
        }

        var assign = assignments.First();

        // בדיקת סטטוס השיוך
        if (assign.assignKind != null)
        {
            throw new Exception($"Assignment for Volunteer ID={volunteerId} and Call ID={callId} has already been closed.");
        }

        // עדכון זמן סיום השיוך
        var updatedAssignment = assign with
        {
            finishTime = DateTime.Now,
            assignKind=DO.Hamal.handeled
        };
        _dal.assignment.Update(updatedAssignment);

        // עדכון סטטוס הקריאה
        var call = _dal.call.Read(callId) ??
            throw new Exception($"Call with ID={callId} does not exist.");

        var updatedCall = call with { maximumTime=DateTime.Now };
        _dal.call.Update(updatedCall);
    }


    public void DeleteCall(int callId)
    {
        // בדיקת קיום הקריאה
        var call = _dal.call.Read(callId) ??
            throw new Exception($"Call with ID={callId} does not exist.");

        // בדיקת אם הקריאה הסתיימה
        var currentTime = DateTime.Now;
        var assignments = _dal.assignment.ReadAll(a => a.callId == callId);

        if (assignments.Any(a => a.finishTime != null) || (call.maximumTime != null && currentTime > call.maximumTime))
        {
            throw new Exception($"Cannot delete a completed call with ID={callId}.");
        }

        // בדיקת שיוכים פעילים לקריאה
        if (assignments.Any())
        {
            throw new Exception($"Cannot delete a call with active assignments (Call ID={callId}).");
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
                status = Status.closed;
            }
            else if (call.startTime.HasValue)
            {
                // אם יש זמן התחלה אך עדיין לא הסתיימה
                status = Status.inProgres;
            }
            else
            {
                // אחרת, הקריאה ממתינה לטיפול
                status = Status.open;
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

    public IEnumerable<OpenCallInList> GetOpenCallsByVolunteer(int volunteerId, Enum? callType = null, Enum? sortField = null)
    {
        // קבלת כל הקריאות הפתוחות (ללא finishTime)
        var calls = _dal.call.ReadAll(c => c.maximumTime > DateTime.Now); // קריאות פתוחות בלבד

        // מציאת המתנדב (בהנחה שמאגר המתנדבים נקרא _dal.volunteer)
        var volunteer = _dal.volunteer.ReadAll().FirstOrDefault(v => v.idVol == volunteerId);
        if (volunteer == null)
        {
            throw new InvalidOperationException("Volunteer not found.");
        }
        if (volunteer.latitude == null || volunteer.longitude == null)
        {
            throw new ArgumentException("Volunteer location is not provided.");
        }
        // מיזוג נתוני הקריאות והשיוכים ליצירת OpenCallInList
        var openCalls = from call in calls
                        let assignment = _dal.assignment.ReadAll()
                            .FirstOrDefault(assign => assign.callId == call.id && assign.finishTime == null)
                        where assignment == null || (assignment.volunteerId == volunteerId && assignment.finishTime == null) // רק קריאות ללא שיוך או קריאות של המתנדב הנוכחי
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

        //// מיזוג נתוני הקריאות והשיוכים ליצירת OpenCallInList
        //var openCalls = from call in calls
        //                let assignment = _dal.assignment.ReadAll()
        //                    .FirstOrDefault(assign => assign.callId == call.id && assign.finishTime == null)
        //                where assignment == null || assignment.volunteerId == volunteerId // כלול קריאות ללא שיוך או קריאות של המתנדב הנוכחי
        //                select new OpenCallInList
        //                {
        //                    Id = call.id,
        //                    Tkoc = (TheKindOfCall)(call.callType ?? 0), // המרת Enum עבור סוג הקריאה
        //                    Description = call.detail,
        //                    Address = call.adress,
        //                    OpenTime = call.startTime ?? DateTime.MinValue,
        //                    MaxEndTime = call.maximumTime,
        //                    DistanceFromVolunteer = CalculateDistance(call.latitude ?? 0, call.longitude ?? 0, volunteer.latitude, volunteer.longitude)
        //                };

        // סינון לפי סוג הקריאה אם צוין
        if (callType != null)
        {
            openCalls = openCalls.Where(c => c.Tkoc == (TheKindOfCall)callType);
        }

        // מיון הקריאות לפי השדה הנבחר
        if (sortField != null && sortField is SortField)
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
            .FirstOrDefault(a => a.volunteerId == volunteerId && a.assignKind == DO.Hamal.handeled);
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

    public void UpdateCallDetails(Call call)
    {
        // וודא שהקריאה אינה null
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
        var coordinates = VolunteerManager.GetCoordinatesFromGoogle(call.Address);
        if (coordinates == null || coordinates.Length < 2)
        {
            throw new InvalidOperationException("Invalid address: could not retrieve coordinates.");
        }

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
        CallManager.Observers.NotifyItemUpdated(doCall.id); //stage 5
        CallManager.Observers.NotifyListUpdated(); //stage 5

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

        // מציאת ההקצאה האחרונה לכל קריאה
        var latestAssignments = assignments
            .GroupBy(a => a.callId)
            .Select(g => g.OrderByDescending(a => a.finishTime).FirstOrDefault());
        AdminImplementation admin = new();
        
        // מיזוג נתוני הקריאות עם ההקצאות
        var callAssignments = from call in calls
                              join assign in latestAssignments on call.id equals assign?.callId into callGroup
                              from assign in callGroup.DefaultIfEmpty()
                              select new CallInList
                              {
                                  CallId = call.id,
                                  CallType = (CallType)(call.callType ?? 0),
                                  OpenTime = call.startTime ?? DateTime.MinValue,
                                  TimeRemaining = call.maximumTime.HasValue
                                      ? call.maximumTime.Value - DateTime.Now
                                      : (TimeSpan?)null,
                                  LastVolunteerName = assign?.volunteerId != null
                                      ? _dal.volunteer.Read(assign.volunteerId)?.name
                                      : null,
                                  CompletionTime = assign?.finishTime != null
                                      ? assign.finishTime.Value - (call.startTime ?? DateTime.MinValue)
                                      : null,
                                  TotalAssignments = assignments.Count(a => a.callId == call.id),
                                  Status = UpdateStatus(ConvertToBOCall(call), admin.GetRiskTimeSpan())
                              };

        // סינון הקריאות לפי שדה וערך (אם נבחרו)
        if (filterField != null && filterValue != null)
        {
            callAssignments = filterField switch
            {
                CallField.Status => callAssignments.Where(c => c.Status == (Status)filterValue),
                CallField.AssignedTo => callAssignments.Where(c => c.LastVolunteerName == (string)filterValue),
                _ => callAssignments
            };
        }

        // מיון הקריאות לפי שדה שנבחר
        if (sortField != null)
        {
            callAssignments = sortField switch
            {
                CallField.Status => callAssignments.OrderBy(c => c.Status),
                CallField.AssignedTo => callAssignments.OrderBy(c => c.LastVolunteerName),
                _ => callAssignments.OrderBy(c => c.CallId)
            };
        }
        else
        {
            callAssignments = callAssignments.OrderBy(c => c.CallId);
        }

        return callAssignments;
    }
    public Status UpdateStatus(Call call, TimeSpan riskTime)
    {
        
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

        // אם לא נמצא סטטוס מתאים, נשאיר את הסטטוס הקיים
        return call.Status;
    }



    public BO.Call ConvertToBOCall(DO.Call doCall)
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



    public void AddObserver(Action listObserver) =>
CallManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
CallManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
CallManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
CallManager.Observers.RemoveObserver(id, observer); //stage 5

    
}