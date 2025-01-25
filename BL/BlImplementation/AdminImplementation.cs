namespace BlImplementation;

using System;
using BlApi;
using BO;
using Helpers;

// <summary>
// מימוש ממשק לניהול מערכת
// </summary>
public class AdminImplementation : IAdmin
{
    private static TimeSpan _riskTimeSpan = TimeSpan.FromMinutes(30); // ערך ברירת מחדל


    private readonly DalApi.Idal _dal = DalApi.Factory.Get;
    // קידום שעון המערכת
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
        AdminManager.UpdateClock(newTime);
    }
    //בקשת טווח זמן סיכון
    public TimeSpan GetRiskTimeSpan()
    {
        return _riskTimeSpan;
    }
    //  בקשת שעון המערכת
    public DateTime GetSystemClock()
    {
        return AdminManager.Now;
    }
    //אתחול בסיס הנתונים
    public void InitializeDB() //stage 4
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.InitializeDB(); //stage 7
    }

    //איפוס בסיס הנתונים
    //public void ResetDatabase()
    //{
    //    _dal.ResetDB();
    //    AdminManager.UpdateClock(AdminManager.Now);
    //}
    public void ResetDB() //stage 4
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.ResetDB(); //stage 7
    }

    //הגדרת טווח זמן סיכון
    public void SetRiskTimeSpan(TimeSpan riskTimeSpan)
    {
        _riskTimeSpan = riskTimeSpan;
    }
    #region Stage 5
    public void AddClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers += clockObserver;
    public void RemoveClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers -= clockObserver;
    public void AddConfigObserver(Action configObserver) =>
   AdminManager.ConfigUpdatedObservers += configObserver;
    public void RemoveConfigObserver(Action configObserver) =>
    AdminManager.ConfigUpdatedObservers -= configObserver;
    #endregion Stage 5
    public void StartSimulator(int interval)  //stage 7
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.Start(interval); //stage 7
    }
    public void StopSimulator()
        => AdminManager.Stop(); //stage 7


}