namespace BO;
using BO.Enums;
public class CallInList
{
    /// <summary>
    /// מספר מזהה של ישות ההקצאה
    /// </summary>
    public int? Id { get; init; }

    /// <summary>
    /// מספר מזהה רץ של ישות הקריאה
    /// </summary>
    public int CallId { get; init; }

    /// <summary>
    /// סוג הקריאה (ENUM)
    /// </summary>
    public CallType CallType { get; set; }

    /// <summary>
    /// זמן פתיחה של הקריאה
    /// </summary>
    public DateTime OpenTime { get; set; }

    /// <summary>
    /// סך זמן שנותר לסיום הקריאה
    /// </summary>
    public TimeSpan? TimeRemaining { get; set; }

    /// <summary>
    /// שם מתנדב אחרון
    /// </summary>
    public string? LastVolunteerName { get; set; }

    /// <summary>
    /// סך זמן השלמת הטיפול
    /// </summary>
    public TimeSpan? CompletionTime { get; set; }

    /// <summary>
    /// סטטוס הקריאה
    /// </summary>
    public Status Status { get; set; }

    /// <summary>
    /// סך הקצאות עבור הקריאה
    /// </summary>
    public int TotalAssignments { get; set; }
}

    