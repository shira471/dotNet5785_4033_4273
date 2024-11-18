namespace Dal;
using DalApi;
using DO;
public class ConfigImplementation : Iconfig
{
    //public DateTime clock
    //{
    //    get { return clock; }
    //    set
    //    {
    //        if (value >= DateTime.Now) // Optional validation
    //        {
    //            clock = value;
    //        }
    //        else
    //        {
    //            throw new ArgumentException("Clock cannot be set to a past time.");
    //        }
    //    }
    //}
    private DateTime _clock=DateTime.Now;
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


    public int GetNextCallId()
    {
        return Config.nextCallId;
    }

    public int getNextCallId()
    {
        return Config.GetNextAssignId;
    }

    public void Reset()
    {
        Config.Reset();
    }
}

