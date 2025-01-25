namespace Dal;

using System.Runtime.CompilerServices;
using DalApi;
using DO;
internal class VolunteerImplementation : Ivolunteer
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    // יצירת מתנדב חדש
    public void Create(Volunteer item)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        if (volunteers.Any(it => it.idVol == item.idVol))
            throw new DalAlreadyExistsException($"Volunteer with ID={item.idVol} already exists");
        volunteers.Add(item);
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // קריאת מתנדב לפי מזהה
    public Volunteer? Read(int id)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        return volunteers.FirstOrDefault(it => it.idVol == id);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // קריאת מתנדב לפי תנאי מסוים
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        return volunteers.FirstOrDefault(filter);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    // קריאת כל המתנדבים
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        return filter == null ? volunteers : volunteers.Where(filter);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]

    // עדכון מתנדב
    public void Update(Volunteer item)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        if (volunteers.RemoveAll(it => it.idVol == item.idVol) == 0)
            throw new DalDoesNotExistException($"Volunteer with ID={item.idVol} does Not exist");
        volunteers.Add(item);
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]

    // מחיקת מתנדב לפי מזהה
    public void Delete(int id)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        if (volunteers.RemoveAll(it => it.idVol == id) == 0)
            throw new DalDoesNotExistException($"Volunteer with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]

    // מחיקת כל המתנדבים
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Volunteer>(), Config.s_volunteers_xml);
    }
}