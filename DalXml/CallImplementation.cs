namespace Dal;

using System.Runtime.CompilerServices;
using DalApi;
using DO;


internal class CallImplementation : Icall
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    // יצירת קריאה חדשה
    public void Create(Call item)
    {
        // טען את רשימת הקריאות מהקובץ
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);

        // השג את המזהה הרץ הבא ועדכן את ה-ID של הקריאה
        int newId = Config.NextCallId; // קבלת המזהה הרץ
        item = item with { id = newId }; // עדכון המזהה בקריאה החדשה

        // הוסף את הקריאה לרשימה
        calls.Add(item);

        // שמור את הרשימה המעודכנת בקובץ
        XMLTools.SaveListToXMLSerializer(calls, Config.s_calls_xml);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // קריאת קריאה לפי מזהה
    public Call? Read(int id)
    {
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        return calls.FirstOrDefault(it => it.id == id);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // קריאת קריאה לפי תנאי
    public Call? Read(Func<Call, bool> filter)
    {
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        return calls.FirstOrDefault(filter);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // קריאת כל הקריאות לפי תנאי (או ללא תנאי)
    //public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    //{
    //    List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
    //    return filter == null ? calls : calls.Where(filter);
    //}
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);

        // בדיקה אם יש קריאות עם שדה maximumTime null
        foreach (var call in calls)
        {
            if (call.maximumTime == null)
            {
                Console.WriteLine($"Call ID {call} has null maximumTime.");
            }
        }

        return filter == null ? calls : calls.Where(filter);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    // עדכון קריאה קיימת
    public void Update(Call item)
    {
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        if (calls.RemoveAll(it => it.id == item.id) == 0)
            throw new DalDoesNotExistException($"Call with ID={item.id} does Not exist");

        calls.Add(item);
        XMLTools.SaveListToXMLSerializer(calls, Config.s_calls_xml);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // מחיקת קריאה לפי מזהה
    public void Delete(int id)
    {
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        if (calls.RemoveAll(it => it.id == id) == 0)
            throw new DalDoesNotExistException($"Call with ID={id} does Not exist");

        XMLTools.SaveListToXMLSerializer(calls, Config.s_calls_xml);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // מחיקת כל הקריאות
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Call>(), Config.s_calls_xml);
    }
}