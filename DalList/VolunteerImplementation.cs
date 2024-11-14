
namespace Dal;

using System.Collections.Generic;
using DalApi;
using DO;
public class VolunteerImplementation : Ivolunteer
{
    public void Create(Volunteer item)
    {
        if (Read(item.idVol) != null)
        {
            throw new Exception($"volunteer object with this ID={item.idVol} already exissts");
        }
        else
        {
            DataSource.volunteers.Add(item);
        }
    }
    public void Delete(int id)
    {
        var isExist = DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol == id);
        if (isExist != null)
        {
            DataSource.volunteers.Remove(isExist);
        }
        else
        {
            throw new Exception($"volunteer with this ID={id} does not exists");
        }
    }

    public void DeleteAll()
    {
        DataSource.volunteers.RemoveAll(v => v is DO.Volunteer);
    }

    public Volunteer? Read(int id)
    {
        var newId= DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol==id);
        if (newId != null)
        {
            return newId;
        }
        else
        {
            throw new Exception($"volunteer with this ID={id} does not exists");
        }
    }

    public List<Volunteer> ReadAll()
    {
        return DataSource.volunteers.ToList();
    }

    public void Update(Volunteer item)
    {
        var isExist = DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol == item.idVol);
        if (isExist != null)
        {
            DataSource.volunteers.Remove(isExist);
            DataSource.volunteers.Add(item);
        }
        else
        {
            throw new Exception($"volunteer with this ID={item.idVol} does not exists");
        }
    }
}
