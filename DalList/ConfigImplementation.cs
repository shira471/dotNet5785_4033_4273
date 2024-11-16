//namespace Dal;
//using DalApi;
//using DO;
//public class ConfigImplementation : Iconfig
//{
//    public DateTime clock {
//        get { return clock; }
//        set
//        {
//            if (value >= DateTime.Now) // Optional validation
//            {
//                clock = value;
//            }
//            else
//            {
//                throw new ArgumentException("Clock cannot be set to a past time.");
//            }
//        }
//    }

//    public int GetNextCallId()
//    {
//        return Config.nextCallId;
//    }

//    public int getNextCallId()
//    {
//        return Config.GetNextAssignId;
//    }

//    public void Reset()
//    {
//        Config.Reset();
//    }
//}

namespace Dal;
using DalApi;
using DO;

public class ConfigImplementation : Iconfig
{
    // שדה פרטי לשמירת ערך השעון
    private DateTime _clock;

    /// <summary>
    /// מאפיין לגישה לערך השעון
    /// </summary>
    public DateTime clock
    {
        get { return _clock; } // החזרת ערך השעון
        set
        {
            if (value >= DateTime.Now) // בדיקת תקינות הערך
            {
                _clock = value; // עדכון ערך השעון
            }
            else
            {
                throw new ArgumentException("Clock cannot be set to a past time."); // זריקת חריגה במקרה של ערך לא חוקי
            }
        }
    }

    /// <summary>
    /// קבלת מזהה קריאה הבא
    /// </summary>
    public int GetNextCallId()
    {
        return Config.nextCallId;
    }


    /// <summary>
    /// קבלת מזהה שיוך הבא
    /// </summary>
    public int getNextCallId()
    {
        return Config.GetNextAssignId;
    }


    /// <summary>
    /// איפוס כל נתוני התצורה
    /// </summary>
    public void Reset()
    {
        Config.Reset();
    }
}
