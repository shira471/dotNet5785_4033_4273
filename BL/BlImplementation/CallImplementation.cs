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
        if(call.CallType==CallType.None)
        {
            throw new ArgumentNullException("Call type cannot be non.");
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
        // בדיקה אם המתנדב כבר ביטל את הקריאה בעבר
        var volunteerCancelledAssignment = _dal.assignment.ReadAll()
            .FirstOrDefault(a => a.callId == callId && a.volunteerId == volunteerId && a.assignKind == DO.Hamal.cancelByVolunteer);

        if (volunteerCancelledAssignment != null)
        {
            throw new Exception($"Volunteer with ID={volunteerId} has already cancelled this call and cannot reassign it.");
        }


        var existingAssignment = _dal.assignment.ReadAll()
       .FirstOrDefault(a => a.callId == callId &&
                            a.assignKind != DO.Hamal.cancelByManager &&
                            (a.assignKind == DO.Hamal.inTreatment || a.assignKind == DO.Hamal.handeled));

        if (existingAssignment != null)
        {
            throw new Exception($"Call with ID={callId} is already assigned to another volunteer.");
        }

        // בדיקה אם למתנדב יש קריאה אחרת במצב "בטיפול"
        var volunteerActiveAssignment = _dal.assignment.ReadAll()
            .FirstOrDefault(a => a.volunteerId == volunteerId && a.assignKind == DO.Hamal.inTreatment);

        if (volunteerActiveAssignment != null)
        {
            throw new Exception($"Volunteer with ID={volunteerId} is already working on another call.");
        }

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
            assignKind = DO.Hamal.inTreatment
        };
        _dal.assignment.Create(assignment);

        // עדכון סטטוס הקריאה
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

    public void CancelCallAssignment(int volunteerId, int callId,BO.Role role)
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

        // קביעת סוג הביטול בהתאם לתפקיד
        Hamal newAssignKind = role switch
        {
            BO.Role.Manager => Hamal.cancelByManager,
            BO.Role.Volunteer => Hamal.cancelByVolunteer,
            _ => throw new Exception($"Role {role} is not authorized to cancel assignments.")
        };

        // עדכון האובייקט הקיים
        var updatedAssignment = assign with
        {
            assignKind = (DO.Hamal)newAssignKind
        };

        _dal.assignment.Update(updatedAssignment);

        // עדכון סטטוס הקריאה
        var x = ConvertToBOCall(call);
        x.Status = Status.open;

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
        if (assign.assignKind != DO.Hamal.inTreatment)
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

        var x = ConvertToBOCall(call);
        x.Status = Status.closed;
    }


    public void DeleteCall(int callId)
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

        // עדכון צופים (אם קיים מנגנון צופים)
        CallManager.Observers.NotifyListUpdated(); //stage 5
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
                              actualEndTime: assign.finishTime??null,
                              endType: (Hamal?)assign.assignKind ??null// המרת Enum
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
        // יצירת רשימת קריאות פתוחות
        var openCalls = from call in calls
                        let assignments = _dal.assignment.ReadAll().Where(a => a.callId == call.id)
                        where
                            !assignments.Any() || // אין שיוכים כלל
                            assignments.All(a =>
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

        // סינון לפי סוג הקריאה אם צוין
        if (callType != null && callType !=  BO.CallType.None)
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
                    if (filterValue is object Status)
                    {
                        callAssignments = callAssignments.Where(c => c.Status == BO.Status.open);
                    }
                    break;

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
    private Status DetermineStatus(DO.Call call, DO.Assignment? assign, TimeSpan riskTimeSpan)
    {
        // אם הזמן עבר ואין הקצאה
        if (assign == null && call.maximumTime < DateTime.Now)
        {
            return Status.expired;
        }

        // אם הזמן עבר ויש הקצאה פעילה
        if (assign != null && call.maximumTime < DateTime.Now && assign.finishTime == null)
        {
            return Status.expired;
        }

        // אם הזמן עבר ויש הקצאה שהסתיימה
        if (assign != null && call.maximumTime < DateTime.Now && assign.finishTime.HasValue)
        {
            return call.maximumTime.HasValue && assign.finishTime.Value > call.maximumTime - riskTimeSpan
                ? Status.closeInRisk
                : Status.closed;
        }

        // קריאה ללא הקצאה והזמן לא עבר
        if (assign == null || assign.assignKind == DO.Hamal.cancelByManager || assign.assignKind == DO.Hamal.cancelByVolunteer)
        {
            return call.maximumTime < DateTime.Now + riskTimeSpan ? Status.openInRisk : Status.open;
        }

        // קריאה עם הקצאה פעילה והזמן לא עבר
        if (assign.finishTime == null)
        {
            return call.maximumTime < DateTime.Now + riskTimeSpan ? Status.openInRisk : Status.inProgres;
        }

        // קריאה שהסתיימה והזמן לא עבר
        if (assign.finishTime.HasValue)
        {
            return call.maximumTime.HasValue && assign.finishTime.Value > call.maximumTime - riskTimeSpan
                ? Status.closeInRisk
                : Status.closed;
        }

        // ברירת מחדל (לא אמור לקרות)
        throw new InvalidOperationException("Unable to determine status for the given input.");
    }
    private Dictionary<int, Status> GetStatusesByCall(IEnumerable<DO.Call> calls, IEnumerable<DO.Assignment> assignments, TimeSpan riskTimeSpan)
    {
        var latestAssignments = assignments
            .GroupBy(a => a.callId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.id).FirstOrDefault()
            );


        return calls.ToDictionary(
                call => call.id,
                call =>
                {
                    var assignment = latestAssignments.TryGetValue(call.id, out var assign) ? assign : null;
                    return DetermineStatus(call, assignment, riskTimeSpan);
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
            .FirstOrDefault(a => a.volunteerId == volunteerId && a.assignKind == DO.Hamal.inTreatment);

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
}