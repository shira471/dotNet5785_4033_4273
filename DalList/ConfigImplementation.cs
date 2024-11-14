namespace Dal;
using DalApi;
using DO;
public class ConfigImplementation : Iconfig
{
    public DateTime clock {
        get { return clock; }
        set
        {
            if (value >= DateTime.Now) // Optional validation
            {
                clock = value;
            }
            else
            {
                throw new ArgumentException("Clock cannot be set to a past time.");
            }
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