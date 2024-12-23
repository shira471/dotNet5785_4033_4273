using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DalApi;
using DO;
using Helpers;

namespace BL.Helpers;

internal static class CallManager
{
    internal static ObserverManager Observers = new(); //stage 5 
    // גישה לשכבת ה-DAL
    private static Idal s_dal = Factory.Get; //stage 4

    /// <summary>
    /// עדכון קריאות על פי לוגיקה בהתאם לשעון המערכת.
    /// </summary>
    /// <param name="oldClock">השעון הקודם</param>
    /// <param name="newClock">השעון החדש</param>
    internal static void PeriodicCallUpdates(DateTime oldClock, DateTime newClock)
    {
        var calls = s_dal.call.ReadAll().ToList();

        foreach (var call in calls)
        {
            // דוגמה: בדיקת קריאה שזמן המקסימלי שלה עבר
            if (call.maximumTime != null && call.maximumTime < newClock)
            {
                // עדכון של זמן המקסימום למצב לא פעיל (לדוגמה)
                var updatedCall = call with { maximumTime = null }; // כאן "null" מסמן מצב לא פעיל
                s_dal.call.Update(updatedCall); // עדכון ב-DAL
                Observers.NotifyItemUpdated(updatedCall.id); //stage 5
            }
        }
    }
}
