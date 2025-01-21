namespace Dal;
using DalApi;
using DO;

internal class ConfigImplementation : Iconfig
{
    // שעון המערכת
    private DateTime _clock = DateTime.Now;

    public DateTime clock
    {
        get { return _clock; } // מחזיר את הזמן הנוכחי של השעון
        set
        {
            // בדיקה אם הזמן החדש הוא בעבר
            if (value < DateTime.Now)
            {
                throw new ArgumentException("Clock cannot be set to a past time.");
            }

            _clock = value; // מעדכן את השעון עם הזמן החדש
        }
    }

    // טווח זמן סיכון
    private TimeSpan _riskTimeRange = TimeSpan.Zero;

    public TimeSpan RiskTimeRange
    {
        get { return _riskTimeRange; } // מחזיר את טווח הזמן הנוכחי
        set { _riskTimeRange = value; } // מעדכן את טווח הזמן
    }

    public int MaxRange { get; set; }

    public int getNextCallId()
    {
        return Config.nextCallId;
    }

    public void Reset()
    {
        Config.Reset();
        _clock = DateTime.Now; // מאתחל את השעון לזמן הנוכחי
        _riskTimeRange = TimeSpan.Zero; // מאתחל את טווח הזמן
    }

    public int GetNextAssignmentId()
    {
        return Config.GetNextAssignId;
    }
}
