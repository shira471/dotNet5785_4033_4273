using DO;
namespace DalApi
{
    public interface Iconfig
    {
        int getNextCallId(); // קבלת מזהה הקריאה הבא
        DateTime clock { set; get; } // שעון המערכת
        TimeSpan RiskTimeRange { get; set; } // טווח זמן סיכון
        void Reset(); // איפוס הגדרות
    }
}
