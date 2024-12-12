using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO.Enums;

namespace BlApi;

/// <summary>
/// ממשק עבור ניהול מערכת
/// </summary>
public interface IAdmin
{
    // 1. בקשת שעון המערכת
    // מחזירה את ערכו הנוכחי של שעון המערכת מטיפוס DateTime.
    DateTime GetSystemClock();

    // 2. קידום שעון המערכת
    // מקבלת פרמטר מטיפוס ENUM המייצג יחידת זמן (דקה, שעה, יום, חודש, שנה).
    // מקדמת את שעון המערכת לפי יחידת הזמן ומעדכנת את השעון באמצעות ClockManager.UpdateClock().
    void AdvanceSystemClock(TimeUnit timeUnit);

    // 3. בקשת טווח זמן סיכון
    // מחזירה את הערך הנוכחי של משתנה התצורה "טווח זמן סיכון" מטיפוס TimeSpan.
    TimeSpan GetRiskTimeSpan();

    // 4. הגדרת טווח זמן סיכון
    // מקבלת פרמטר מטיפוס TimeSpan ומעדכנת את ערך משתנה התצורה "טווח זמן סיכון".
    void SetRiskTimeSpan(TimeSpan riskTimeSpan);

    // 5. איפוס בסיס הנתונים
    // מאפסת את כל נתוני התצורה ומנקה את כל הישויות (רשימות הנתונים).
    void ResetDatabase();

    // 6. אתחול בסיס הנתונים
    // מאפסת את בסיס הנתונים ולאחר מכן מאתחלת אותו עם נתוני ברירת מחדל.
    void InitializeDatabase();
}