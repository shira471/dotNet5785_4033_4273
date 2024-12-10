using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using BO;
using BlApi;
using BO.Enums;
using DalApi;



namespace BlImplementation
{
    /// <summary>
    /// מימוש ממשק לניהול מערכת
    /// </summary>
    internal class AdminImplementation : IAdmin
    {
        // גישה לשכבת הנתונים
        //private readonly IDal _dal = DalApi.Factory.Get();
        private readonly DalApi.Idal _dal = DalApi.Factory.Get;


        /// <summary>
        /// קבלת שעון המערכת
        /// </summary>
        /// <returns>הזמן הנוכחי של המערכת</returns>
        public DateTime GetClock()
        {
            // מחזיר את הזמן הנוכחי של שעון המערכת
            return ClockManager.Now;
        }

        /// <summary>
        /// קידום שעון המערכת
        /// </summary>
        /// <param name="unit">יחידת זמן לקידום</param>
        public void ForwardClock(BO.Enums.TimeUnit unit)
        {
            // חישוב הזמן החדש על פי יחידת הזמן שהתקבלה
            DateTime newTime = unit switch
            {
                BO.Enums.TimeUnit.Minute => ClockManager.Now.AddMinutes(1),
                BO.Enums.TimeUnit.Hour => ClockManager.Now.AddHours(1),
                BO.Enums.TimeUnit.Day => ClockManager.Now.AddDays(1),
                BO.Enums.TimeUnit.Month => ClockManager.Now.AddMonths(1),
                BO.Enums.TimeUnit.Year => ClockManager.Now.AddYears(1),
                _ => throw new ArgumentException("יחידת הזמן אינה מוכרת.", nameof(unit)),
            };

            // עדכון שעון המערכת לזמן החדש
            ClockManager.UpdateClock(newTime);
        }

        /// <summary>
        /// קבלת טווח זמן סיכון
        /// </summary>
        /// <returns>טווח זמן סיכון</returns>
        public TimeSpan GetRiskTimeRange()
        {
            // מחזיר את טווח הזמן הנוכחי שנמצא במערכת
            return _dal.Config.RiskTimeRange;
        }

        /// <summary>
        /// הגדרת טווח זמן סיכון
        /// </summary>
        /// <param name="riskTime">טווח זמן סיכון</param>
        public void SetRiskTimeRange(TimeSpan riskTime)
        {
            // עדכון טווח הזמן במשתנה התצורה של שכבת הנתונים
            _dal.Config.RiskTimeRange = riskTime;
        }

        /// <summary>
        /// אתחול בסיס נתונים
        /// </summary>
        public void InitializeDB()
        {
            // קריאה לאתחול בסיס הנתונים
            DalTest.Initialization.Do();

            // עדכון שעון המערכת לזמן הנוכחי לאחר האתחול
            ClockManager.UpdateClock(ClockManager.Now);
        }

        /// <summary>
        /// איפוס בסיס נתונים
        /// </summary>
        public void ResetDB()
        {
            // קריאה לאיפוס בסיס הנתונים
            _dal.ResetDB();

            // עדכון שעון המערכת לזמן הנוכחי לאחר האיפוס
            ClockManager.UpdateClock(ClockManager.Now);
        }
    }
}
