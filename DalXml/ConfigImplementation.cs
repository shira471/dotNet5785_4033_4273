namespace Dal;
using DalApi;
using DO;
internal class ConfigImplementation : Iconfig
{

    private DateTime _clock = DateTime.Now;
    public DateTime clock
    {
        get { return _clock; } // מחזיר את הזמן הנוכחי של השעון
        set
        {

            _clock = value; // מעדכן את השעון עם הזמן החדש
        }
    }

    //public TimeSpan RiskTimeRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    private TimeSpan _riskTimeRange = TimeSpan.FromDays(0); // ערך ברירת מחדל

    public TimeSpan RiskTimeRange
    {
        get => _riskTimeRange;
        set
        {
            // עדכון השדה
            _riskTimeRange = value;
        }
    }

    public int MaxRange { get; set; }

    public int GetNextAssignmentId()
    {
        return (int)Config.NextAssignmentId; // מחזיר את הערך של המספר הרץ הבא עבור Assignments מ- Config
    }

    public int getNextCallId()
    {
        return Config.NextCallId; // גם כאן מחזיר את המספר הרץ הבא עבור Assignments
    }


    public void Reset()
    {
        Config.Reset();
    }
}


