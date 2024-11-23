namespace Dal;

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
        var newId = DataSource.calls.FirstOrDefault(a => a.id == id);
        if (newId != null)
            return newId;
        else
        {
            throw new DalDoesNotExistException($"call with this ID={id} does not exists");
        }
    }

    public List<Call> ReadAll()
    {
        return DataSource.calls.ToList();
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