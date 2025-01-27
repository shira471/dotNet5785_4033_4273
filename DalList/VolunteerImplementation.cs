
namespace Dal;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DalApi;
using DO;

internal class VolunteerImplementation : Ivolunteer
{
    /// <summary>
    /// Create a new volunteer and add it to the data source.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Volunteer item)
    {
        if (Read(item.idVol) != null)
        {
            throw new DalAlreadyExistsException($"volunteer object with this ID={item.idVol} already exists");
        }
        else
        {
            DataSource.volunteers.Add(item);
        }
    }

    /// <summary>
    /// Delete a volunteer by ID.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        var isExist = DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol == id);
        if (isExist != null)
        {
            DataSource.volunteers.Remove(isExist);
        }
        else
        {
            throw new DalDoesNotExistException($"volunteer with this ID={id} does not exist");
        }
    }

    /// <summary>
    /// Delete all volunteers from the data source.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        DataSource.volunteers.RemoveAll(v => v is DO.Volunteer);
    }
    //Read a volunteer
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Volunteer? Read(int id)
    {
        // Use LINQ's FirstOrDefault method to find a volunteer by ID.
        var volunteer = DataSource.volunteers.FirstOrDefault(v => v.idVol == id);

        // Log the result to the console.
        if (volunteer != null)
        {
            Console.WriteLine($"Volunteer found: {volunteer}");
        }
        else
        {
            Console.WriteLine($"Volunteer with ID={id} does not exist.");
        }

        // Return the found volunteer or null if not found.
        return volunteer;
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {

        // Use LINQ's FirstOrDefault method to find the first matching volunteer.
        return DataSource.volunteers.FirstOrDefault(filter);

    }

    [MethodImpl(MethodImplOptions.Synchronized)]

    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null)
    {
        // If no filter is provided, return all volunteers as an enumerable.
        // If a filter is provided, return only the volunteers that match the filter using LINQ's Where method.
        return filter == null
            ? DataSource.volunteers.AsEnumerable() // Return all volunteers as an IEnumerable.
            : DataSource.volunteers.Where(filter); // Apply the filter and return the matching volunteers.
    }

    /// <summary>
    /// Update an existing volunteer.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Volunteer item)
    {
        // Validate phone number format
        if (string.IsNullOrWhiteSpace(item.phoneNumber) || !Regex.IsMatch(item.phoneNumber, @"^\d{9,10}$"))
        {
            throw new DalImposiblePhoneNumber($"Invalid phone number format for volunteer ID={item.idVol}");
        }

        var isExist = DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol == item.idVol);
        if (isExist != null)
        {
            DataSource.volunteers.Remove(isExist);
            DataSource.volunteers.Add(item);
        }
        else
        {
            throw new DalDoesNotExistException($"volunteer with this ID={item.idVol} does not exist");
        }
    }
}