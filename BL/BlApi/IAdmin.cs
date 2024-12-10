using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO.Enums;


namespace BlApi
{
    /// <summary>
    /// ממשק עבור ניהול מערכת
    /// </summary>
    public interface IAdmin
    {
        /// <summary>
        /// קבלת שעון המערכת
        /// </summary>
        /// <returns>הזמן הנוכחי של המערכת</returns>
        DateTime GetClock();

        /// <summary>
        /// קידום שעון המערכת
        /// </summary>
        /// <param name="unit">יחידת זמן לקידום</param>
        void ForwardClock(BO.Enums.TimeUnit unit);

        /// <summary>
        /// קבלת טווח זמן סיכון
        /// </summary>
        /// <returns>טווח זמן סיכון</returns>
        TimeSpan GetRiskTimeRange();

        /// <summary>
        /// הגדרת טווח זמן סיכון
        /// </summary>
        /// <param name="riskTime">טווח זמן סיכון</param>
        void SetRiskTimeRange(TimeSpan riskTime);

        /// <summary>
        /// אתחול בסיס נתונים
        /// </summary>
        void InitializeDB();

        /// <summary>
        /// איפוס בסיס נתונים
        /// </summary>
        void ResetDB();
    }
}
