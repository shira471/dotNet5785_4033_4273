﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BO;
public class VolunteerInList
{
    // ת.ז מתנדב (לא ניתן לעדכון)
    public int Id { get; init; }

    // שם מלא (פרטי ומשפחה)
    public string FullName { get; init; }
    //מספר טלפון של המתנדב
    public string Phone {  get; set; }

    //המייל של המתנדב
    public string mail { get; set; }

    // האם המתנדב פעיל
    public bool IsActive { get; set; }

    // סך קריאות שטופלו על ידי המתנדב
    public int TotalCallsHandled { get; set; }

    // סך קריאות שבוטלו על ידי המתנדב
    public int TotalCallsCancelled { get; set; }

    // סך קריאות שפג תוקפן על ידי המתנדב
    public int TotalExpiredCalls { get; set; }

    // מספר מזהה של הקריאה בטיפולו (אם קיימת)
    public int? CurrentCallId { get; set; }

    // סוג הקריאה שבטיפולו (אם לא קיימת קריאה בטיפולו, יהיה ערך None)
    public CallType CurrentCallType { get; set; }

}

