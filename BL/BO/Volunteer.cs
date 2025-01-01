using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BO;

public class Volunteer
{
    // ת.ז. מתנדב (לא ניתן לעדכון לאחר ההוספה)
    public int Id { get; init; }

    // שם מלא (פרטי ומשפחה)
    public string FullName { get; set; }

    // טלפון סלולרי
    public string Phone { get; set; }

    // אימייל
    public string Email { get; set; }

    // סיסמה (עם הצפנה ותקינות)
    public string? Password { get; set; }

    // כתובת מלאה נוכחית
    public string? Address { get; set; }

    // קו רוחב
    public double? Latitude { get; set; }

    // קו אורך
    public double? Longitude { get; set; }

    // תפקיד
    public Role? Role { get; set; }

    // האם המתנדב פעיל
    public bool IsActive { get; set; }

    // מרחק מרבי לקבלת קריאה
    public double? MaxDistance { get; set; }

    // סוג המרחק
    public DistanceType DistanceType { get; set; }

    // סך הקריאות שטיפל בהן
    public int TotalCallsHandled { get; set; }

    // סך הקריאות שביטל
    public int TotalCallsCancelled { get; set; }

    // סך הקריאות שבחר לטפל ופג תוקפן
    public int TotalExpiredCalls { get; set; }

    // קריאה בטיפול מתנדב
    public CallInProgress? CurrentCall { get; set; }

  
}

