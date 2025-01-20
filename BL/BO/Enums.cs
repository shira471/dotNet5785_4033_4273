//namespace BO.Enums;
namespace BO;
/// <summary>
/// אפשרויות מיון למתנדבים
/// </summary>
public enum VolunteerSortBy
{
    Id,
    Name,
    ActivityStatus
}


// Enum לתפקיד
public enum Role
{
    Manager,
    Volunteer
}

// Enum לסוג המרחק
public enum DistanceType
{
    AirDistance,
    WalkingDistance,
    DrivingDistance
}

public enum SortField
{
    Id,              // מיון לפי מספר קריאה
    Address,         // מיון לפי כתובת
    OpenTime,        // מיון לפי זמן פתיחת הקריאה
    MaxFinishTime,   // מיון לפי הזמן המקסימלי של הקריאה
    DistanceOfCall   // מיון לפי מרחק מהמתנדב
}

/// <summary>
/// סוג הקריאה
/// </summary>
public enum CallType
{
    Emergency,
    Routine,
    Maintenance,
    None
}
/// <summary>
/// טיפוס Enum ליחידות זמן (דקה, שעה, יום, שנה).
/// </summary>
public enum TimeUnit
{
    Minute, // דקה
    Hour,   // שעה
    Day,    // יום
    Month, // חודש
    Year    // שנה
}
public enum CallField
{
    None,
    Status,
    AssignedTo,
    Priority
}

/// <summary>
/// סטטוס הקריאה
/// </summary>
public enum Status
{
    Open = 0,
    Closed = 1,
    InProgrese=2
}

/// <summary>
/// סוגי הקריאות האפשריים (דוגמת ENUM)
/// </summary>
public enum TheKindOfCall
{
    Medical,
    Technical,
    Social,
    Other
}
