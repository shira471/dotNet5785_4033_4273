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
            TimeUnit.Minute => ClockManager.Now.AddMinutes(1),
            TimeUnit.Hour => ClockManager.Now.AddHours(1),
            TimeUnit.Day => ClockManager.Now.AddDays(1),
            TimeUnit.Month => ClockManager.Now.AddMonths(1),
            TimeUnit.Year => ClockManager.Now.AddYears(1),
            _ => throw new ArgumentException("Invalid time unit", nameof(timeUnit))
        };
        ClockManager.UpdateClock(newTime);
    }
    //בקשת טווח זמן סיכון
    public TimeSpan GetRiskTimeSpan()
    {
        return _riskTimeSpan;
    }
    //  בקשת שעון המערכת
    public DateTime GetSystemClock()
    {
        return ClockManager.Now;
    }
    //אתחול בסיס הנתונים
    public void InitializeDatabase()
    {
        ResetDatabase();
        DalTest.Initialization.Do();
        ClockManager.UpdateClock(DateTime.Now);
    }
    //איפוס בסיס הנתונים
    public void ResetDatabase()
    {
        _dal.ResetDB();
        ClockManager.UpdateClock(DateTime.Now);
    }
    //הגדרת טווח זמן סיכון
    public void SetRiskTimeSpan(TimeSpan riskTimeSpan)
    {
        _riskTimeSpan = riskTimeSpan;
    }
}