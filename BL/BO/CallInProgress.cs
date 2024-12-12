using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO.Enums;
namespace BO;

    public class CallInProgress
    {
        /// <summary>
        /// מספר מזהה של ישות ההקצאה
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// מספר מזהה רץ של ישות הקריאה
        /// </summary>
        public int CallId { get; init; }

        /// <summary>
        /// סוג הקריאה (ENUM)
        /// </summary>
        public CallType CallType { get; set; }

        /// <summary>
        /// תיאור מילולי
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// כתובת מלאה של הקריאה
        /// </summary>
        public string FullAddress { get; set; }

        /// <summary>
        /// זמן פתיחה של הקריאה
        /// </summary>
        public DateTime OpenTime { get; set; }

        /// <summary>
        /// זמן מקסימלי לסיום הקריאה
        /// </summary>
        public DateTime? MaxCloseTime { get; set; }

        /// <summary>
        /// זמן כניסה לטיפול
        /// </summary>
        public DateTime EntryTime { get; set; }

        /// <summary>
        /// מרחק הקריאה מהמתנדב המטפל
        /// </summary>
        public double DistanceFromVolunteer { get; set; }

        /// <summary>
        /// סטטוס הקריאה
        /// </summary>
        public CallStatus Status { get; set; }
    }

    /// <summary>
    /// סטטוס הקריאה
    /// </summary>
    public enum CallStatus
    {
        InProgress,
        InProgressAtRisk
    }
