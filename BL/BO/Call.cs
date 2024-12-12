using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO.Enums;

namespace BO;

public class Call
{
    /// <summary>
    /// מספר מזהה רץ של ישות הקריאה
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// סוג הקריאה (ENUM)
    /// </summary>
    public CallType CallType { get; set; }

    /// <summary>
    /// תיאור מילולי של הקריאה
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// כתובת מלאה של הקריאה
    /// </summary>
    public string Address { get; init; }

    /// <summary>
    /// קו רוחב של הכתובת
    /// </summary>
    public double? Latitude { get; init; }

    /// <summary>
    /// קו אורך של הכתובת
    /// </summary>
    public double? Longitude { get; init; }

    /// <summary>
    /// זמן פתיחה של הקריאה
    /// </summary>
    public DateTime OpenTime { get; set; }

    /// <summary>
    /// זמן מקסימלי לסיום הקריאה
    /// </summary>
    public DateTime? MaxEndTime { get; set; }

    /// <summary>
    /// סטטוס הקריאה
    /// </summary>
    public Status Status { get; set; }

    /// <summary>
    /// רשימת הקצאות עבור הקריאה
    /// </summary>
    public List<CallAssignInList>? Assignments { get; set; }
}


