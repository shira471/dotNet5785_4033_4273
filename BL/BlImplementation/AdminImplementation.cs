namespace BlImplementation;

using System;
using BL.Helpers;
using BlApi;
using BO;
using Helpers;

/// <summary>
/// Implementation of the admin system interface
/// </summary>
public class AdminImplementation : IAdmin
{
    private static TimeSpan _riskTimeSpan = TimeSpan.FromMinutes(30); // Default value
    private readonly DalApi.Idal _dal = DalApi.Factory.Get;

     //Advance the system clock
    public void AdvanceSystemClock(TimeUnit timeUnit)
    {
        DateTime newTime = timeUnit switch
        {
            TimeUnit.Minute => AdminManager.Now.AddMinutes(1),
            TimeUnit.Hour => AdminManager.Now.AddHours(1),
            TimeUnit.Day => AdminManager.Now.AddDays(1),
            TimeUnit.Month => AdminManager.Now.AddMonths(1),
            TimeUnit.Year => AdminManager.Now.AddYears(1),
            _ => throw new ArgumentException("Invalid time unit", nameof(timeUnit))
        };

        // Update the system clock with lock protection
        lock (AdminManager.BlMutex) // Stage 7
        {
            AdminManager.UpdateClock(newTime);

        }
        // התראה למאזינים
        CallManager.Observers.NotifyListUpdated();
    }
   
    // Request risk time span
    public TimeSpan GetRiskTimeSpan()
    {
        // Lock to ensure thread-safe access to _riskTimeSpan
        lock (AdminManager.BlMutex) // Stage 7
        {
            return _riskTimeSpan;
        }
    }

    // Request the system clock
    public DateTime GetSystemClock()
    {
        // Lock to ensure thread-safe access to AdminManager.Now
        lock (AdminManager.BlMutex) // Stage 7
        {
            return AdminManager.Now;
        }
    }

    // Initialize the database
    public void InitializeDB() // Stage 4
    {
        lock (AdminManager.BlMutex) // Stage 7
        {
            AdminManager.ThrowOnSimulatorIsRunning(); // Stage 7
            AdminManager.InitializeDB(); // Stage 7
        }
    }

    // Reset the database
    public void ResetDB() // Stage 4
    {
        lock (AdminManager.BlMutex) // Stage 7
        {
            AdminManager.ThrowOnSimulatorIsRunning(); // Stage 7
            AdminManager.ResetDB(); // Stage 7
        }
    }

    // Set the risk time span
    public void SetRiskTimeSpan(TimeSpan riskTimeSpan)
    {
        // Lock to ensure thread-safe update to _riskTimeSpan
        lock (AdminManager.BlMutex) // Stage 7
        {
            _riskTimeSpan = riskTimeSpan;
        }
    }

    #region Stage 5: Observer Management
    public void AddClockObserver(Action clockObserver)
    {
        lock (AdminManager.BlMutex) // Stage 7
        {
            AdminManager.ClockUpdatedObservers += clockObserver;
        }
    }

    public void RemoveClockObserver(Action clockObserver)
    {
        lock (AdminManager.BlMutex) // Stage 7
        {
            AdminManager.ClockUpdatedObservers -= clockObserver;
        }
    }

    public void AddConfigObserver(Action configObserver)
    {
        lock (AdminManager.BlMutex) // Stage 7
        {
            AdminManager.ConfigUpdatedObservers += configObserver;
        }
    }

    public void RemoveConfigObserver(Action configObserver)
    {
        lock (AdminManager.BlMutex) // Stage 7
        {
            AdminManager.ConfigUpdatedObservers -= configObserver;
        }
    }
    #endregion Stage 5

    // Start the simulator
    public void StartSimulator(int interval) // Stage 7
    {
        lock (AdminManager.BlMutex) // Stage 7
        {
            AdminManager.ThrowOnSimulatorIsRunning(); // Stage 7
            AdminManager.Start(interval); // Stage 7
        }
    }

    // Stop the simulator
    public void StopSimulator()
    {
        lock (AdminManager.BlMutex) // Stage 7
        {
            AdminManager.Stop(); // Stage 7
        }
    }

}
