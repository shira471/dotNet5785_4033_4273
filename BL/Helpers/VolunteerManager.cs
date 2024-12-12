using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DalApi;
using DO;

using BL.Helpers;

internal static class VolunteerManager
{
    // גישה לשכבת ה-DAL
    private static Idal s_dal = Factory.Get; //stage 4

    /// <summary>
    /// עדכון מצב מתנדבים על פי לוגיקה בהתאם לשעון המערכת.
    /// </summary>
    /// <param name="oldClock">השעון הקודם</param>
    /// <param name="newClock">השעון החדש</param>
    internal static void PeriodicVolunteersUpdates(DateTime oldClock, DateTime newClock)
    {
        var volunteers = s_dal.volunteer.ReadAll().ToList();

        foreach (var volunteer in volunteers)
        {
            // דוגמה: בדיקה אם מתנדב לא פעיל במשך זמן מסוים
            if (!volunteer.isActive && (newClock - oldClock).Days > 30)
            {
                // יצירת עותק חדש של המתנדב עם עדכון isActive
                var updatedVolunteer = volunteer with { isActive = false };
                s_dal.volunteer.Update(updatedVolunteer); // עדכון ב-DAL
            }
        }
    }
}
