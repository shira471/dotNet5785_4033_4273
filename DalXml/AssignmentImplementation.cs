namespace Dal;

using System.Runtime.CompilerServices;
using DalApi;
using DO;

internal class AssignmentImplementation : Iassignment
{
    // יצירת משימה חדשה
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Assignment item)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
      
        int newId = Config.NextAssignmentId; // קבלת המזהה הרץ
        item = item with { id = newId }; // עדכון המזהה במשימה החדשה

        // הוסף את המשימה לרשימה
        assignments.Add(item);

        // שמור את הרשימה המעודכנת בקובץ
        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // קריאה של משימה לפי מזהה ID
    public Assignment? Read(int id)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return assignments.FirstOrDefault(it => it.id == id);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // קריאה של משימה לפי פילטר מותאם אישית
    public Assignment? Read(Func<Assignment, bool> filter)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return assignments.FirstOrDefault(filter);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // קריאה של כל המשימות לפי פילטר מותאם אישית או ללא פילטר
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return filter == null ? assignments : assignments.Where(filter);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // עדכון של משימה קיימת
    public void Update(Assignment item)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        int index = assignments.FindIndex(it => it.id == item.id);
        if (index == -1)
            throw new DalDoesNotExistException($"Assignment with ID={item.id} does Not exist");

        assignments[index] = item;
        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // מחיקה של משימה לפי מזהה ID
    public void Delete(int id)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        if (assignments.RemoveAll(it => it.id == id) == 0)
            throw new DalDoesNotExistException($"Assignment with ID={id} does Not exist");

        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // מחיקה של כל המשימות
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Assignment>(), Config.s_assignments_xml);
    }
}