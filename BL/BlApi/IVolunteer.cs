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
namespace BlApi;

/// <summary>
/// ממשק עבור ניהול מתנדבים
/// </summary>
public interface IVolunteer: IObservable //stage 5 הרחבת ממשק

{
    /// <summary>
    /// מתודת כניסה למערכת
    /// </summary>
    /// <param name="username">שם המשתמש</param>
    /// <param name="password">סיסמה</param>
    /// <returns>תפקיד המשתמש</returns>
    /// <exception cref="InvalidCredentialsException">נזרקת אם שם המשתמש או הסיסמה שגויים</exception>
    string Login(string username, string password);

    /// <summary>
    /// בקשת רשימת מתנדבים
    /// </summary>
    /// <param name="isActive">מסנן לפי מתנדבים פעילים/לא פעילים (nullable)</param>
    /// <param name="sortBy">שדה מיון לפי ENUM (nullable)</param>
    /// <returns>רשימה מסוננת וממוינת של מתנדבים</returns>
    IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActive = null, VolunteerSortBy? sortBy = null);

    /// <summary>
    /// בקשת פרטי מתנדב
    /// </summary>
    /// <param name="volunteerId">מספר תעודת זהות של המתנדב</param>
    /// <returns>אובייקט מתנדב</returns>
    /// <exception cref="NotFoundException">נזרקת אם לא נמצא מתנדב עם תעודת הזהות שנמסרה</exception>
    BO.Volunteer GetVolunteerDetails(int volunteerId);

    /// <summary>
    /// עדכון פרטי מתנדב
    /// </summary>
    /// <param name="requesterId">ת.ז של המבקש לעדכן</param>
    /// <param name="volunteer">אובייקט מתנדב עם הפרטים המעודכנים</param>
    /// <exception cref="UnauthorizedAccessException">נזרקת אם למבקש אין הרשאה</exception>
    /// <exception cref="InvalidDataException">נזרקת אם הפרטים אינם תקינים</exception>
    void UpdateVolunteer(int requesterId, BO.Volunteer volunteer);

    /// <summary>
    /// בקשת מחיקת מתנדב
    /// </summary>
    /// <param name="volunteerId">מספר תעודת זהות של המתנדב</param>
    /// <exception cref="InvalidOperationException">נזרקת אם לא ניתן למחוק את המתנדב</exception>
    void DeleteVolunteer(int volunteerId);

    /// <summary>
    /// הוספת מתנדב חדש
    /// </summary>
    /// <param name="volunteer">אובייקט מתנדב עם הפרטים</param>
    /// <exception cref="InvalidDataException">נזרקת אם הפרטים אינם תקינים</exception>
    /// <exception cref="DuplicateException">נזרקת אם כבר קיים מתנדב עם תעודת הזהות</exception>
    void AddVolunteer(BO.Volunteer volunteer);
    int GetVolunteerForCall(int callId);
}

   


