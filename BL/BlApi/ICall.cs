using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO;

namespace BlApi;

/// <summary>
/// ממשק עבור ניהול קריאות
/// </summary>
public interface ICall : IObservable //stage 5 הרחבת ממשק

{
    CallInProgress? GetActiveAssignmentForVolunteer(int volunteerId);
    IEnumerable<CallAssignInList> GetAssignmentsForCall(int callId);
    Call? GetAssignedCallByVolunteer(int volunteerId);
    // 1. בקשת כמויות קריאות
    // מחזירה מערך של מספר הקריאות לפי סטטוס הקריאה. 
    // משתמשת ב-GroupBy או LINQ group by.
    int[] GetCallCountsByStatus();

    // 2. בקשת רשימת קריאות
    // מקבלת פרמטרים לסינון ומיון, מחזירה רשימת קריאות מסוננת וממוינת.
    // אם ערך סינון או מיון הוא null, ברירת המחדל היא ללא סינון/מיון לפי מספר קריאה.
    IEnumerable<BO.CallInList> GetCallsList(CallField? filterField, object? filterValue, CallField? sortField);

    // 3. בקשת פרטי קריאה
    // מקבלת מזהה קריאה, מחזירה את פרטי הקריאה כולל רשימת ההקצאות.
    // אם הקריאה לא קיימת, נזרקת חריגה לשכבת התצוגה.
    BO.Call GetCallDetails(string calldes);

    // 4. עדכון פרטי קריאה
    // מקבלת אובייקט קריאה מעודכן, מבצעת בדיקות תקינות נתונים (פורמט ולוגיקה) ומעדכנת את הקריאה בשכבת הנתונים.
    // במידה ואין קריאה עם מזהה כזה, נזרקת חריגה.
    void UpdateCallDetails(BO.Call call);

    // 5. מחיקת קריאה
    // מוחקת קריאה רק אם היא בסטטוס פתוח ולא הוקצתה בעבר.
    // אם לא ניתן למחוק, נזרקת חריגה מתאימה לשכבת התצוגה.
    void DeleteCall(int callId);

    // 6. הוספת קריאה
    // מוסיפה קריאה חדשה לאחר בדיקות תקינות נתונים (פורמט ולוגיקה).
    // אם כבר קיימת קריאה עם מזהה זהה, נזרקת חריגה.
    void AddCall(BO.Call call);

    // 7. רשימת קריאות סגורות לפי מתנדב
    // מחזירה את כל הקריאות הסגורות שטופלו על ידי מתנדב מסוים.
    // מאפשרת סינון לפי סוג קריאה ומיון לפי שדה מסוים.
    IEnumerable<BO.ClosedCallInList> GetClosedCallsByVolunteer(int volunteerId, Enum? callType, Enum? sortField);

    // 8. רשימת קריאות פתוחות לפי מתנדב
    // מחזירה קריאות פתוחות לבחירה עבור מתנדב מסוים, כולל חישוב מרחק מהמתנדב.
    // מאפשרת סינון לפי סוג קריאה ומיון לפי שדה מסוים.
     IEnumerable<BO.OpenCallInList> GetOpenCallsByVolunteer(int volunteerId, CallType? callType = null, Enum? sortField = null);
 
    // 9. עדכון "סיום טיפול" בקריאה
    // מסיימת טיפול בקריאה על ידי מתנדב, מעדכנת את זמן הסיום וסוג הסיום כ"טופלה".
    // אם הנתונים לא חוקיים (למשל, הקצאה לא קיימת או שכבר טופלה), נזרקת חריגה.
    void CloseCallAssignment(int volunteerId, int assignmentId);

    // 10. עדכון "ביטול טיפול" בקריאה
    // מבטלת טיפול בקריאה. אם המבקש הוא המתנדב עצמו, סוג הביטול הוא "ביטול עצמי".
    // אם המבקש הוא מנהל, סוג הביטול הוא "ביטול מנהל".
    // אם הנתונים לא חוקיים, נזרקת חריגה מתאימה.
    void CancelCallAssignment(int requesterId, int assignmentId,BO.Role role);

    // 11. בחירת קריאה לטיפול
    // מקצה קריאה למתנדב, מעדכנת את זמן הכניסה לטיפול.
    // אם הקריאה לא מתאימה לטיפול (למשל, כבר טופלה או פג תוקפה), נזרקת חריגה.
    void AssignCallToVolunteer(int volunteerId, int callId);
}