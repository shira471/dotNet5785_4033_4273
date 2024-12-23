namespace BlImplementation;

using System;
using BlApi;
using BO.Enums;
using Helpers;

// <summary>
// מימוש ממשק לניהול מערכת
// </summary>
internal class AdminImplementation : IAdmin
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
    public void InitializeDatabase()
    {
        ResetDatabase();
        DalTest.Initialization.Do();
        AdminManager.UpdateClock(AdminManager.Now);
        AdminManager.MaxRange = AdminManager.MaxRange;
    }
    //איפוס בסיס הנתונים
    public void ResetDatabase()
    {
        _dal.ResetDB();
        AdminManager.UpdateClock(AdminManager.Now);
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

}