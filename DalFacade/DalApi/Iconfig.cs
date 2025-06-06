﻿using DO;
namespace DalApi
{
    public interface Iconfig
    {
        int getNextCallId(); // קבלת מזהה הקריאה הבא
        int GetNextAssignmentId();
        DateTime clock { set; get; } // שעון המערכת
        TimeSpan RiskTimeRange { get; set; } // טווח זמן סיכון
        int MaxRange { get; set; }

        void Reset(); // איפוס הגדרות
    }
}
