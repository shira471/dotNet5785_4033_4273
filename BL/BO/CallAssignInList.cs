using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO;

public class CallAssignInList
{
    /// <summary>
    /// ת.ז מתנדב
    /// </summary>
    public int? VolunteerId { get; init; }

    /// <summary>
    /// שם מתנדב
    /// </summary>
    public string? VolunteerName { get; set; }

    /// <summary>
    /// זמן כניסה לטיפול
    /// </summary>
    public DateTime EntryTime { get; set; }

    /// <summary>
    /// זמן סיום הטיפול בפועל
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// סוג סיום הטיפול
    /// </summary>
    public AssignmentEndType? EndType { get; set; }
}

    /// <summary>
    /// סוגי סיום הטיפול
    /// </summary>
    public enum AssignmentEndType
    {
        Completed,
        CancelledByVolunteer,
        Expired,
        OpenCancelled
    }

