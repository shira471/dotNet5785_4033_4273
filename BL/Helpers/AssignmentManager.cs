using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DalApi;
using DO;
using Helpers;

namespace BL.Helpers;

internal static class AssignmentManager
{
    internal static ObserverManager Observers = new(); //stage 5 
    // גישה לשכבת ה-DAL
    private static Idal s_dal = Factory.Get;

    /// <summary>
    /// מתודה לעדכון משימות על פי לוגיקה בהתאם לשעון המערכת
    /// </summary>
    /// <param name="oldClock">השעה הקודמת</param>
    /// <param name="newClock">השעה החדשה</param>
    internal static void PeriodicAssignmentUpdates(DateTime oldClock, DateTime newClock)
    {
        // קריאה לכל המשימות
        var assignments = s_dal.assignment.ReadAll().ToList();

        foreach (var assignment in assignments)
        {
            // דוגמה: אם יש משימה ללא זמן סיום, נעדכן אותה
            if (!assignment.finishTime.HasValue && assignment.startTime.HasValue)
            {
                // אם השעה החדשה עברה את השעה המשוערת לסיום
                if ((assignment.startTime.Value.AddHours(4)) < newClock)
                {
                    // עדכון זמן הסיום
                    var updatedAssignment = assignment with { finishTime = newClock };
                    s_dal.assignment.Update(updatedAssignment);
                }
            }
        }
    }
}
