namespace Dal;
using DalApi;
using DO;

internal class AssignmentImplementation : Iassignment
{
    // יצירת משימה חדשה
    public void Create(Assignment item)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        if (assignments.Any(it => it.id == item.id))
            throw new DalAlreadyExistsException($"Assignment with ID={item.id} already exists");

        assignments.Add(item);
        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);
    }

    // קריאה של משימה לפי מזהה ID
    public Assignment? Read(int id)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return assignments.FirstOrDefault(it => it.id == id);
    }

    // קריאה של משימה לפי פילטר מותאם אישית
    public Assignment? Read(Func<Assignment, bool> filter)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return assignments.FirstOrDefault(filter);
    }

    // קריאה של כל המשימות לפי פילטר מותאם אישית או ללא פילטר
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return filter == null ? assignments : assignments.Where(filter);
    }

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

    // מחיקה של משימה לפי מזהה ID
    public void Delete(int id)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        if (assignments.RemoveAll(it => it.id == id) == 0)
            throw new DalDoesNotExistException($"Assignment with ID={id} does Not exist");

        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);
    }

    // מחיקה של כל המשימות
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Assignment>(), Config.s_assignments_xml);
    }
}