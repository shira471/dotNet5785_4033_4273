using DO;
namespace DalApi
{

    public interface Iconfig
    {
        int getNextCallId();
        DateTime clock { set; get; }
        void Reset();
    }
}