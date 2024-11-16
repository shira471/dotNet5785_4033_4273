//namespace Dal;

//using System.Collections.Generic;
//using DalApi;
//using DO;
//public class VolunteerImplementation : Ivolunteer
//{
//    public void Create(Volunteer item)
//    {
//        if (Read(item.idVol) != null)
//        {
//            throw new Exception($"volunteer object with this ID={item.idVol} already exissts");
//        }
//        else
//        {
//            DataSource.volunteers.Add(item);
//        }
//    }
//    public void Delete(int id)
//    {
//        var isExist = DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol == id);
//        if (isExist != null)
//        {
//            DataSource.volunteers.Remove(isExist);
//        }
//        else
//        {
//            throw new Exception($"volunteer with this ID={id} does not exists");
//        }
//    }

//    public void DeleteAll()
//    {
//        DataSource.volunteers.RemoveAll(v => v is DO.Volunteer);
//    }

//    public Volunteer? Read(int id)
//    {
//        var newId= DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol==id);
//        if (newId != null)
//        {
//            return newId;
//        }
//        else
//        {
//            throw new Exception($"volunteer with this ID={id} does not exists");
//        }
//    }

//    public List<Volunteer> ReadAll()
//    {
//        return DataSource.volunteers.ToList();
//    }

//    public void Update(Volunteer item)
//    {
//        var isExist = DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol == item.idVol);
//        if (isExist != null)
//        {
//            DataSource.volunteers.Remove(isExist);
//            DataSource.volunteers.Add(item);
//        }
//        else
//        {
//            throw new Exception($"volunteer with this ID={item.idVol} does not exists");
//        }
//    }
//}


namespace Dal;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using DalApi;
using DO;

public class VolunteerImplementation : Ivolunteer
{
    /// <summary>
    /// Create a new volunteer and add it to the data source.
    /// </summary>
    public void Create(Volunteer item)
    {
        // Validate phone number format
        if (string.IsNullOrWhiteSpace(item.phoneNumber) || !Regex.IsMatch(item.phoneNumber, @"^\d{9,10}$"))
        {
            throw new Exception($"Invalid phone number format for volunteer ID={item.idVol}");
        }

        if (Read(item.idVol) != null)
        {
            throw new Exception($"volunteer object with this ID={item.idVol} already exists");
        }
        else
        {
            DataSource.volunteers.Add(item);
        }
    }

    /// <summary>
    /// Delete a volunteer by ID.
    /// </summary>
    public void Delete(int id)
    {
        var isExist = DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol == id);
        if (isExist != null)
        {
            DataSource.volunteers.Remove(isExist);
        }
        else
        {
            throw new Exception($"volunteer with this ID={id} does not exist");
        }
    }

    /// <summary>
    /// Delete all volunteers from the data source.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.volunteers.RemoveAll(v => v is DO.Volunteer);
    }

    /// <summary>
    /// Read a volunteer by ID.
    /// </summary>
    //public Volunteer? Read(int id)
    //{
    //    var newId = DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol == id);
    //    if (newId != null)
    //    {
    //        return newId;
    //    }
    //    else
    //    {
    //        throw new Exception($"volunteer with this ID={id} does not exist");
    //    }
    //}
    public Volunteer? Read(int id)
    {
        try
        {
            // בדיקה אם DataSource.volunteers אינו ריק
            if (DataSource.volunteers == null || !DataSource.volunteers.Any())
            {
                Console.WriteLine("DataSource.volunteers is empty or not initialized.");
                return null;
            }

            // הדפסת כל המתנדבים במאגר לצורך בדיקה
            Console.WriteLine("Current volunteers in DataSource:");
            foreach (var volunteer in DataSource.volunteers)
            {
                Console.WriteLine(volunteer);
            }

            // חיפוש מתנדב לפי ID
            var volunteerToFind = DataSource.volunteers.FirstOrDefault(v => v.idVol == id);

            // אם נמצא מתנדב, להחזיר אותו
            if (volunteerToFind != null)
            {
                Console.WriteLine($"Volunteer found: {volunteerToFind}");
                return volunteerToFind;
            }
            else
            {
                // במקרה של אי-מציאת מתנדב
                Console.WriteLine($"Volunteer with ID={id} does not exist.");
                return null;
            }
        }
        catch (Exception ex)
        {
            // טיפול בחריגות והדפסה למסך
            Console.WriteLine($"Error in Read function: {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// Read all volunteers from the data source.
    /// </summary>
    public List<Volunteer> ReadAll()
    {
        return DataSource.volunteers.ToList();
    }

    /// <summary>
    /// Update an existing volunteer.
    /// </summary>
    public void Update(Volunteer item)
    {
        // Validate phone number format
        if (string.IsNullOrWhiteSpace(item.phoneNumber) || !Regex.IsMatch(item.phoneNumber, @"^\d{9,10}$"))
        {
            throw new Exception($"Invalid phone number format for volunteer ID={item.idVol}");
        }

        var isExist = DataSource.volunteers.FirstOrDefault(volunteers => volunteers.idVol == item.idVol);
        if (isExist != null)
        {
            DataSource.volunteers.Remove(isExist);
            DataSource.volunteers.Add(item);
        }
        else
        {
            throw new Exception($"volunteer with this ID={item.idVol} does not exist");
        }
    }
}
