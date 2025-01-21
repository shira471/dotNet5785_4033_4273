namespace Dal;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DalApi;
using DO;
internal class AssignmentImplementation : Iassignment
{
    public void Create(Assignment item)
    {
        int newId = Config.GetNextAssignId;
        Assignment newItem = item with { id = newId };
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
            throw new DalDoesNotExistException($"assignment with this ID={id} does not exists");
        }
    }

    public void DeleteAll()
    {
        DataSource.assignments.RemoveAll(v => v is DO.Assignment);
    }
    public Assignment? Read(int id)
    {
        // Use LINQ's FirstOrDefault method to find the assignment by ID.
        var assignment = DataSource.assignments.FirstOrDefault(a => a.id == id);

        // If the assignment is not found, throw an exception.
        if (assignment == null)
        {
            throw new DalDoesNotExistException($"Assignment with ID={id} does not exist.");
        }

        // Return the found assignment.
        return assignment;
    }

    public Assignment? Read(Func<Assignment, bool> filter)
    {
        // Use LINQ's FirstOrDefault method to find the first matching volunteer.
        return DataSource.assignments.FirstOrDefault(filter);
    }

    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
    {
        // If no filter is provided, return all volunteers as an enumerable.
        // If a filter is provided, return only the volunteers that match the filter using LINQ's Where method.
        return filter == null
            ? DataSource.assignments.AsEnumerable() // Return all volunteers as an IEnumerable.
            : DataSource.assignments.Where(filter); // Apply the filter and return the matching volunteers.
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
            throw new DalDoesNotExistException($"assignment with this ID={item.id} does not exists");
        }

    }

}