using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO.Enums;
namespace BO;
public class VolunteerInList
{
    // ת.ז מתנדב (לא ניתן לעדכון)
    public int Id { get; init; }

    // שם מלא (פרטי ומשפחה)
    public string FullName { get; init; }

    // האם המתנדב פעיל
    public bool IsActive { get; init; }

    // סך קריאות שטופלו על ידי המתנדב
    public int TotalCallsHandled { get; init; }

    // סך קריאות שבוטלו על ידי המתנדב
    public int TotalCallsCancelled { get; init; }

    // סך קריאות שפג תוקפן על ידי המתנדב
    public int TotalExpiredCalls { get; init; }

    // מספר מזהה של הקריאה בטיפולו (אם קיימת)
    public int? CurrentCallId { get; init; }

    // סוג הקריאה שבטיפולו (אם לא קיימת קריאה בטיפולו, יהיה ערך None)
    public CallType CurrentCallType { get; init; }

    // קונסטרוקטור
    public VolunteerInList(int id, string fullName, bool isActive, int totalCallsHandled=0,
                            int totalCallsCancelled = 0, int totalExpiredCalls = 0, int? currentCallId=null ,
                            CallType currentCallType=0)
    {
        Id = id;
        FullName = fullName;
        IsActive = isActive;
        TotalCallsHandled = totalCallsHandled;
        TotalCallsCancelled = totalCallsCancelled;
        TotalExpiredCalls = totalExpiredCalls;
        CurrentCallId = currentCallId;
        if (currentCallType == null)// ברירת מחדל אם אין קריאה בטיפולו
            CurrentCallType = CallType.None;
        else CurrentCallType = currentCallType;
    }
}

