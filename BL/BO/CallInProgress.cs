using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum CallStatus
    {
        InProgress,       // בטיפול כרגע
        AtRisk            // בטיפול בסיכון
    }
    public class CallInProgress
    {
        // Properties
        int Id; // מספר מזהה של ישות ההקצאה
        int CallId; // מספר מזהה של הקריאה
        string CallType; // סוג הקריאה
        string Description; // תיאור מילולי
        string Address;// כתובת מלאה של הקריאה
        DateTime OpenTime; // זמן פתיחה
        DateTime? MaxCompletionTime; // זמן מקסימלי לסיום הקריאה
         DateTime AssignmentStartTime; // זמן כניסה לטיפול
        double DistanceFromVolunteer; // מרחק מהמתנדב
        CallStatus Status; // סטטוס הקריאה

        // Constructor
        public CallInProgress(int id, int callId, string callType, string description, string address,
            DateTime openTime, DateTime? maxCompletionTime, DateTime assignmentStartTime, double distanceFromVolunteer, CallStatus status)
        {
            Id = id;
            CallId = callId;
            CallType = callType;
            Description = description;
            Address = address;
            OpenTime = openTime;
            MaxCompletionTime = maxCompletionTime;
            AssignmentStartTime = assignmentStartTime;
            DistanceFromVolunteer = distanceFromVolunteer;
            Status = status;
        }
    }
}
    
