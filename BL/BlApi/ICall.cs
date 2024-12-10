using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlApi
{
    /// <summary>
    /// ממשק עבור ניהול קריאות
    /// </summary>
    public interface ICall
    {
        /// <summary>
        /// בקשת כמויות קריאות לפי סטטוס
        /// </summary>
        /// <returns>מערך של כמויות קריאות</returns>
        int[] GetCallsCountByStatus();

        /// <summary>
        /// בקשת רשימת קריאות
        /// </summary>
        /// <param name="filterField">שדה לסינון</param>
        /// <param name="filterValue">ערך סינון</param>
        /// <param name="sortField">שדה מיון</param>
        /// <returns>רשימת קריאות</returns>
        IEnumerable<BO.CallInList> GetCallsList(BO.CallSortField? filterField, object? filterValue, BO.CallSortField? sortField);

        /// <summary>
        /// בקשת פרטי קריאה
        /// </summary>
        /// <param name="id">מזהה קריאה</param>
        /// <returns>פרטי קריאה</returns>
        BO.Call GetCallDetails(int id);

        /// <summary>
        /// עדכון פרטי קריאה
        /// </summary>
        /// <param name="call">פרטי קריאה לעדכון</param>
        void UpdateCall(BO.Call call);

        /// <summary>
        /// מחיקת קריאה
        /// </summary>
        /// <param name="id">מזהה קריאה</param>
        void DeleteCall(int id);

        /// <summary>
        /// הוספת קריאה
        /// </summary>
        /// <param name="call">פרטי קריאה חדשה</param>
        void AddCall(BO.Call call);
    }
}
