using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO.Enums;
namespace BO;

public class OpenCallInList
{
    /// <summary>
    /// מספר מזהה רץ של ישות הקריאה
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// סוג הקריאה (ENUM)
    /// </summary>
    public TheKindOfCall Tkoc { get; set; }

    /// <summary>
    /// תיאור מילולי של הקריאה
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// כתובת מלאה של הקריאה
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// זמן פתיחה של הקריאה
    /// </summary>
    public DateTime OpenTime { get; set; }

    /// <summary>
    /// זמן מקסימלי לסיום הקריאה (יכול להיות null)
    /// </summary>
    public DateTime? MaxEndTime { get; set; }

    /// <summary>
    /// מרחק הקריאה מהמתנדב (מחושב בשכבה הלוגית)
    /// </summary>
    public double DistanceFromVolunteer { get; set; }
}



