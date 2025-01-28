using System;
using System.Collections.Generic;
using System.Linq;
using DalApi;
using DO;
using Helpers;

namespace BL.Helpers;

internal static class CallManager
{
    private static int s_periodicCounter = 0;
    internal static ObserverManager Observers = new(); // Stage 5
    // Access to DAL
    private static Idal s_dal = Factory.Get; // Stage 4

    /// <summary>
    /// Update calls according to the logic, based on the system clock.
    /// </summary>
    /// <param name="oldClock">The previous clock.</param>
    /// <param name="newClock">The new clock.</param>
    internal static void PeriodicCallUpdates(DateTime oldClock, DateTime newClock)
    {
        List<Call> calls;

        // Lock for reading all calls
        lock (AdminManager.BlMutex) // Stage 7
        {
            calls = s_dal.call.ReadAll().ToList();
        }
        List<int> updatedCallIds = new();
        foreach (var call in calls)
        {
            // Check if a call's maximum time has passed
            if (call.maximumTime != null && call.maximumTime < newClock)
            {
                // Update the call's maximum time to null (inactive state)
                var updatedCall = call;

                // Lock for updating the call in DAL
                lock (AdminManager.BlMutex) // Stage 7
                {
                    s_dal.call.Update(updatedCall);
                }
                updatedCallIds.Add(updatedCall.id);
                // Notify observers outside the lock
               // Observers.NotifyItemUpdated(updatedCall.id); // Stage 5
            }
        }
        // עדכון כל המשקיפים (מחוץ לנעילה)
        foreach (var callId in updatedCallIds)
        {
            Observers.NotifyItemUpdated(callId);
        }

        if (updatedCallIds.Any())
        {
            Observers.NotifyListUpdated(); // עדכון כללי אם הרשימה השתנתה
        }
    }

    internal static void SimulateCallActivity(DateTime startClock, DateTime endClock)
    {
        Thread.CurrentThread.Name = $"SimulationThread{++s_periodicCounter}"; // Optional for debugging

        List<Call> calls;

        // Lock for reading all calls, converting to concrete list to avoid deferred query execution
        lock (AdminManager.BlMutex) // Lock for DAL access
        {
            calls = s_dal.call.ReadAll().ToList(); // Convert to a concrete list
        }

        List<int> updatedCallIds = new List<int>(); // Collect IDs for notification

        // Perform simulation over the time period
        for (DateTime currentTime = startClock; currentTime <= endClock; currentTime = currentTime.AddDays(1))
        {
            foreach (var call in calls)
            {
                // Example: Automatically close calls if they exceed the maximum time
                if (call.maximumTime.HasValue && currentTime > call.maximumTime)
                {
                    var updatedCall = call with { maximumTime = null }; // Example: Set maximumTime to null to indicate closure

                    // Lock for updating the call in DAL
                    lock (AdminManager.BlMutex) // Lock per update
                    {
                        s_dal.call.Update(updatedCall);
                    }

                    // Collect the updated call's ID for notification
                    updatedCallIds.Add(updatedCall.id);
                }
            }

        }

        // Notify observers outside the lock
        foreach (var callId in updatedCallIds)
        {
            Observers.NotifyItemUpdated(callId);
        }

        // Optionally, notify that the whole list was updated if there were changes
        if (updatedCallIds.Any())
        {
            Observers.NotifyListUpdated();
        }
    }

    internal static void PeriodicCallUpdates()
    {
        throw new NotImplementedException();
    }
}
