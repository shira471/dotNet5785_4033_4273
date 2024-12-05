using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum CallType
    {
        Emergency,
        Assistance,
        Transport,
        None // למקרה שאין קריאה בטיפול
    }
    internal class VolunteerInList
    {
        // Properties
        int Id; // ת.ז מתנדב
        string FullName; // שם מלא
        bool IsActive; // האם פעיל
        int TotalHandledCalls; // סך הקריאות שטופלו
        int TotalCancelledCalls; // סך הקריאות שבוטלו
        int TotalExpiredCalls; // סך הקריאות שפג תוקפן
        int? CurrentCallId; // מספר מזהה של הקריאה בטיפולו
        CallType CurrentCallType; // סוג הקריאה בטיפולו

        // Constructor
        public VolunteerInList(int id, string fullName, bool isActive, int totalHandledCalls,
            int totalCancelledCalls, int totalExpiredCalls, int? currentCallId, CallType currentCallType)
        {
            Id = id;
            FullName = fullName;
            IsActive = isActive;
            TotalHandledCalls = totalHandledCalls;
            TotalCancelledCalls = totalCancelledCalls;
            TotalExpiredCalls = totalExpiredCalls;
            CurrentCallId = currentCallId;
            CurrentCallType = currentCallType;
        }
    }
}
