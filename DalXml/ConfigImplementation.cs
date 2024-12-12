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
            // כאן אפשר להוסיף כל בדיקה שצריך, לדוגמה: לא לאפשר להגדיר זמן עבר
            if (value < DateTime.Now)
            {
                throw new ArgumentException("Clock cannot be set to a past time.");
            }

            _clock = value; // מעדכן את השעון עם הזמן החדש
        }
    }

    public TimeSpan RiskTimeRange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public int GetNextCallId()
    {
        return Config.NextAssignmentId; // מחזיר את הערך של המספר הרץ הבא עבור Assignments מ- Config
    }

    public int getNextCallId()
    {
        return Config.NextAssignmentId; // גם כאן מחזיר את המספר הרץ הבא עבור Assignments
    }


    public void Reset()
    {
        Config.Reset();
    }
}



