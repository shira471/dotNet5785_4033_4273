using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO;
using DO;
using DalApi;
using System.Linq;
using System.Text.RegularExpressions;

namespace BlApi
{
    /// <summary>
    /// ממשק עבור ניהול מתנדבים
    /// </summary>
    public interface IVolunteer
    {
        /// <summary>
        /// מתודת כניסה למערכת
        /// </summary>
        /// <param name="username">שם משתמש</param>
        /// <param name="password">סיסמה</param>
        /// <returns>תפקיד המשתמש</returns>
        BO.Role Login(string username, string password);

        /// <summary>
        /// בקשת רשימת מתנדבים
        /// </summary>
        /// <param name="isActiveFilter">סינון לפי פעיל/לא פעיל</param>
        /// <param name="sortField">שדה מיון</param>
        /// <returns>רשימת מתנדבים</returns>
        IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActiveFilter, BO.VolunteerSortField? sortField);

        /// <summary>
        /// בקשת פרטי מתנדב
        /// </summary>
        /// <param name="id">מזהה מתנדב</param>
        /// <returns>פרטי מתנדב</returns>
        BO.Volunteer GetVolunteerDetails(int id);

        /// <summary>
        /// עדכון פרטי מתנדב
        /// </summary>
        /// <param name="id">מזהה מתנדב</param>
        /// <param name="volunteer">פרטי מתנדב לעדכון</param>
        void UpdateVolunteer(int id, BO.Volunteer volunteer);

        /// <summary>
        /// מחיקת מתנדב
        /// </summary>
        /// <param name="id">מזהה מתנדב</param>
        void DeleteVolunteer(int id);

        /// <summary>
        /// הוספת מתנדב
        /// </summary>
        /// <param name="volunteer">פרטי מתנדב חדש</param>
        void AddVolunteer(BO.Volunteer volunteer);
    }
}
