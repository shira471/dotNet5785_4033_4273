using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using DalApi;
using DO;
using Helpers;
using BlImplementation;

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

  
    internal static void PeriodicCallUpdates(DateTime oldClock, DateTime newClock)
    {
        // Set thread name for easier debugging
        // Thread.CurrentThread.Name = $"PeriodicCallUpdates"; ??? to review

        // Local list to store IDs for notifications outside the lock
        List<int> expiredCallIds = new();

        // Step 1: Retrieve all calls from the data source
        List<DO.Call> activeCalls;
        lock (AdminManager.BlMutex) // Lock for data retrieval
        {
            activeCalls = s_dal.call.ReadAll()
                                   .Where(call => call.maximumTime > oldClock && call.maximumTime <= newClock)
                                   .ToList();
        }

        // Step 2: Process calls and perform updates
        foreach (var call in activeCalls)
        {
            // Assuming these are expired calls that require updates
            lock (AdminManager.BlMutex) // Lock for database updates
            {
                s_dal.call.Update(call with { maximumTime = null }); // Update the call
            }

            // Add the call ID to the local list for notifications
            expiredCallIds.Add(call.id);
        }

        // Step 3: Send notifications outside the lock
        foreach (var callId in expiredCallIds)
        {
            Observers.NotifyItemUpdated(callId); // Notify about the specific updated item
        }

        // Step 4: Check if the list requires a global update notification
        if (oldClock.Year != newClock.Year || expiredCallIds.Any())
        {
            Observers.NotifyListUpdated(); // Notify about a global list update
        }
    }

    //internal static async Task SendCancelationMail(DO.Assignment a)
    //{
    //    var fromAddress = new MailAddress("auviwin3@gmail.com");
    //    MailAddress? toAddress = null;
    //    lock (AdminManager.BlMutex)
    //        toAddress = new MailAddress(s_dal.volunteer.Read(a.volunteerId)!.email, s_dal.volunteer.Read(a.volunteerId)!.name);
    //    const string fromPassword = "pate iojy wgxd qkjx";
    //    const string subject = "Assignment Cancelation";
    //    string body = "Your assignment is no longer under your treatment!\nThank you for your service.\nReason: " + a.finishTime.ToString();

    //    var smtp = new SmtpClient
    //    {
    //        Host = "smtp.gmail.com",
    //        Port = 587,
    //        EnableSsl = true,
    //        DeliveryMethod = SmtpDeliveryMethod.Network,
    //        UseDefaultCredentials = false,
    //        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
    //    };
    //    using (var message = new MailMessage(fromAddress, toAddress)
    //    {
    //        Subject = subject,
    //        Body = body
    //    })
    //    {
    //        await smtp.SendMailAsync(message);
    //    }
    //}


    //internal static async Task SendCallOpenMail(BO.Call call)
    //{
    //    IEnumerable<DO.Volunteer> doVolunteers;
    //    lock (AdminManager.BlMutex) //stage 7
    //        doVolunteers = s_dal.volunteer.ReadAll();

    //    var Volunteers = from Volunteer in doVolunteers
    //                     where Volunteer.limitDestenation <= CallImplementation.CalculateDistance((double)Volunteer.latitude, (double)Volunteer.longitude, (double)call.Latitude, (double)call.Longitude)
    //                     where Volunteer.IsActive == true
    //                     select Volunteer;

    //    foreach (var Volunteer in Volunteers)
    //    {
    //        var fromAddress = new MailAddress("shimon78900@gmail.com");
    //        MailAddress? toAddress = null;
    //        lock (AdminManager.BlMutex)
    //            toAddress = new MailAddress(s_dal.volunteer.Read(Volunteer.Id)!.Email, s_dal.volunteer.Read(Volunteer.Id)!.FullName);
    //        const string fromPassword = "pate iojy wgxd qkjx";
    //        const string subject = "New Call Open in your area";
    //        string body = "This call is open in your area!\n" + call.ToString();

    //        var smtp = new SmtpClient
    //        {
    //            Host = "smtp.gmail.com",
    //            Port = 587,
    //            EnableSsl = true,
    //            DeliveryMethod = SmtpDeliveryMethod.Network,
    //            UseDefaultCredentials = false,
    //            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
    //        };
    //        using (var message = new MailMessage(fromAddress, toAddress)
    //        {
    //            Subject = subject,
    //            Body = body
    //        })
    //        {
    //            await smtp.SendMailAsync(message);
    //        }
    //    }
    //}
    internal static bool IsVolunteerBusy(int volunteerId)
    {
        lock (AdminManager.BlMutex) // stage 7
        {
            var v = s_dal.volunteer.Read(volunteerId);
            var assignments = s_dal.assignment.ReadAll().Where(a => a.volunteerId == volunteerId && a.assignKind == null);
            return assignments.Any();
        }
    }
