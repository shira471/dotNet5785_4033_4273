using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO;
using DalApi;
using DO;
using Helpers;

namespace BL.Helpers;

internal static class AssignmentManager
{
    private static int s_periodicCounter = 0;
    internal static ObserverManager Observers = new(); //stage 5 
    // גישה לשכבת ה-DAL
    private static Idal s_dal = Factory.Get;

    /// <summary>
    /// מתודה לעדכון משימות על פי לוגיקה בהתאם לשעון המערכת
    /// </summary>
    /// <param name="oldClock">השעה הקודמת</param>
    /// <param name="newClock">השעה החדשה</param>
    internal static void SimulateAssignmentActivity(DateTime startClock, DateTime endClock)
    {
        Thread.CurrentThread.Name = $"SimulationThread{++s_periodicCounter}"; // Optional for debugging

        List<Assignment> assignments;

        // Lock for reading all assignments, converting to concrete list to avoid deferred query execution
        lock (AdminManager.BlMutex) // Lock for DAL access
        {
            assignments = s_dal.assignment.ReadAll().ToList(); // Convert to a concrete list
        }

        List<int> updatedAssignmentIds = new List<int>(); // Collect IDs for notification

        // Perform simulation over the time period
        for (DateTime currentTime = startClock; currentTime <= endClock; currentTime = currentTime.AddDays(1))
        {
            foreach (var assignment in assignments)
            {
                // Example: Automatically close assignments if the finish time has passed
                if (assignment.finishTime.HasValue && currentTime > assignment.finishTime)
                {
                    var updatedAssignment = assignment with { finishTime = null }; // Example: Reset finishTime to indicate it's closed

                    // Lock for updating the assignment in DAL
                    lock (AdminManager.BlMutex) // Lock per update
                    {
                        s_dal.assignment.Update(updatedAssignment);
                    }

                    // Collect the updated assignment's ID for notification
                    updatedAssignmentIds.Add(updatedAssignment.id);
                }
            }
        }

        // Notify observers outside the lock
        foreach (var assignmentId in updatedAssignmentIds)
        {
            Observers.NotifyItemUpdated(assignmentId);
        }

        // Optionally, notify that the whole list was updated if there were changes
        if (updatedAssignmentIds.Any())
        {
            Observers.NotifyListUpdated();
        }
    }

    internal static void PeriodicAssignmentUpdates(DateTime oldClock, DateTime newClock)
    {
        List<Assignment> assignments;

        // קריאת כל המשימות עם נעילה
        lock (AdminManager.BlMutex) //stage 7
        {
            assignments = s_dal.assignment.ReadAll().ToList();
        }
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
                   // s_dal.assignment.Update(updatedAssignment);

                    // עדכון המשימה עם נעילה
                    lock (AdminManager.BlMutex) //stage 7
                    {
                        s_dal.assignment.Update(updatedAssignment);
                    }
                    // שליחת התראה מחוץ לנעילה
                    Observers.NotifyItemUpdated(updatedAssignment.id);
                }
            }
        }
    }
    
}
