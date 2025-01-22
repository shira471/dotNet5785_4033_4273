using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO;
public class ClosedCallInList
{
    // מספר מזהה רץ של ישות הקריאה
    public int Id { get; init; }

    // סוג הקריאה
    public CallType CallType { get; init; }

    // כתובת מלאה של הקריאה
    public string Address { get; init; }

    // זמן פתיחה
    public DateTime OpenTime { get; init; }

    // זמן כניסה לטיפול
    public DateTime AssignmentStartTime { get; init; }

    // זמן סיום הטיפול בפועל (אופציונלי)
    public DateTime? ActualEndTime { get; init; }

    // סוג סיום הטיפול (אופציונלי)
    public Hamal? EndType { get; init; }

    // קונסטרוקטור
    public ClosedCallInList(int id, CallType callType, string address, DateTime openTime,
                            DateTime assignmentStartTime, DateTime? actualEndTime, Hamal? endType)
    {
        Id = id;
        CallType = callType;
        Address = address;
        OpenTime = openTime;
        AssignmentStartTime = assignmentStartTime;
        ActualEndTime = actualEndTime;
        EndType = endType;
    }
}


// Enum לסוג סיום הטיפול
public enum EndType
{
    Completed,   // טיפול הושלם
    Cancelled,   // טיפול בוטל
    Expired      // טיפול פג תוקף
}


