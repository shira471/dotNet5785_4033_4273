using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    internal class ClosedCallInList
    {
        // מזהה קריאה ייחודי
        int Id; // לקוח מישות DO.Call

        /// סוג הקריאה (לדוגמה: חירום, טכנית וכו')
        CallTypeEnum CallType;// לקוח מישות DO.Call

        // כתובת מלאה של הקריאה
        string FullAddress; // לקוח מישות DO.Call

        
        // זמן פתיחה של הקריאה
        
        DateTime OpenTime; // לקוח מישות DO.Call

        
        // זמן כניסה לטיפול
       
        DateTime EntryTime; // לקוח מישות DO.Assignment

        
        // זמן סיום הטיפול בפועל (יכול להיות null אם לא הושלם)
        
        DateTime? CloseTime; // לקוח מישות DO.Assignment


        // סוג סיום הטיפול (לדוגמה: הושלם, בוטל, פג תוקף)

        CallEndTypeEnum? EndType;// לקוח מישות DO.Assignment
    }

    
    // Enum עבור סוגי קריאות
    
    public enum CallTypeEnum
    {
        Emergency, // קריאת חירום
        Standard,  // קריאה רגילה
        Technical  // קריאה טכנית
    }

    
    // Enum עבור סוגי סיום טיפול
    
    public enum CallEndTypeEnum
    {
        Handled,    // הקריאה טופלה
        Cancelled,  // הקריאה בוטלה
        Expired     // הקריאה פג תוקפה
    }
}

