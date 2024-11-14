
namespace Dal;

using System.Collections.Generic;
using System.Collections.Immutable;
using DalApi;
using DO;
public class AssignmentImplementation : Iassignment
{
    public void Create(Assignment item)
    {
        int newId = Config.GetNextAssignId;
        Assignment newItem= item with { id = newId };
        DataSource.assignments.Add(newItem);
    }

    public void Delete(int id)
    {
        var assignmentToDelete = DataSource.assignments.FirstOrDefault(a => a.id == id);
        if (assignmentToDelete != null)
        {
            DataSource.assignments.Remove(assignmentToDelete);
        }
        else
        {
            throw new Exception($"assignment with this ID={id} does not exists");
        }
    }

    public void DeleteAll()
    {
        DataSource.assignments.RemoveAll(v => v is DO.Assignment);
    }

    public Assignment? Read(int id)
    {
        var newId=DataSource.assignments.FirstOrDefault(a => a.id == id);
        if(newId != null)
        {
            return newId;
        }
        else
        {
            throw new Exception($"assignment with this ID={id} does not exists");
        }
    }

    public List<Assignment> ReadAll()
    {
        return DataSource.assignments.ToList();
    }

    public void Update(Assignment item)
    {
        var existingAssignment = DataSource.assignments.FirstOrDefault(a => a.id == item.id);
        if (existingAssignment != null)
        {
            DataSource.assignments.Remove(existingAssignment);
            DataSource.assignments.Add(item);
        }
        else
        {
            throw new Exception($"assignment with this ID={item.id} does not exists");
        }

    }
}
