namespace Dal;

using System;
using System.Collections.Generic;
using DalApi;
using DO;
internal class CallImplementation : Icall
{
    public void Create(Call item)
    {
        int newId = Config.nextCallId;
        Call newItem = item with { id = newId };
        DataSource.calls.Add(newItem);
    }

    public void Delete(int id)
    {
        var CallToDelete = DataSource.calls.FirstOrDefault(a => a.id == id);
        if (CallToDelete != null)
        {
            DataSource.calls.Remove(CallToDelete);
        }
        else
        {
            throw new DalDoesNotExistException($"call with this ID={id} does not exists");
        }
    }

    public void DeleteAll()
    {
        DataSource.calls.RemoveAll(v => v is DO.Call);
    }


    public Call? Read(int id)
    {
        // Use LINQ's FirstOrDefault method to find a volunteer by ID.
        var call = DataSource.calls.FirstOrDefault(v => v.id == id);

        // Log the result to the console.
        if (call != null)
        {
            Console.WriteLine($"Call found: {call}");
        }
        else
        {
            Console.WriteLine($"Call with ID={id} does not exist.");
        }

        // Return the found volunteer or null if not found.
        return call;
    }

    public Call? Read(Func<Call, bool> filter)
    {
        // Use LINQ's FirstOrDefault method to find the first matching volunteer.
        return DataSource.calls.FirstOrDefault(filter);

    }

    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
        // If no filter is provided, return all volunteers as an enumerable.
        // If a filter is provided, return only the volunteers that match the filter using LINQ's Where method.
        return filter == null
            ? DataSource.calls.AsEnumerable() // Return all volunteers as an IEnumerable.
            : DataSource.calls.Where(filter); // Apply the filter and return the matching volunteers.
    }

    public void Update(Call item)
    {
        var existingCall = DataSource.calls.FirstOrDefault(a => a.id == item.id);
        if (existingCall != null)
        {
            DataSource.calls.Remove(existingCall);
            DataSource.calls.Add(item);
        }
        else
        {
            throw new DalDoesNotExistException($"call with this ID={item.id} does not exists");
        }
    }
}