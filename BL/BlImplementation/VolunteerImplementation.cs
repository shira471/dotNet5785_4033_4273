namespace BlImplementation;
using System.Net;
using System.Text.Json;
using BlApi;
using BO;
//using System;
using System.Net.Http;
using System.Threading.Tasks;
using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BL.Helpers;
using System.Data.Common;
using DO;
using DalApi;
using System.Data;
using System.Collections.Generic;

//using Newtonsoft.Json.Linq;
public class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.Idal _dal = DalApi.Factory.Get;

    public void AddVolunteer(BO.Volunteer volunteer)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        if (volunteer == null)
        {
            throw new BlNullPropertyException("Volunteer object cannot be null.");
        }

        if (!VolunteerManager.IsValidEmail(volunteer.Email))
        {
            throw new BlInvalidValueException("Invalid email format.");
        }

        if (!VolunteerManager.IsValidPhoneNumber(volunteer.Phone))
        {
            throw new BlInvalidValueException("Invalid phone number.");
        }

        if (!VolunteerManager.IsValidId(volunteer.Id))
        {
            throw new BlInvalidValueException("Invalid ID number.");
        }

        if (volunteer.MaxDistance < 0)
            throw new ArgumentException("MaxDistance must be non-negative.");

        try
        {
            var temp = VolunteerManager.GetCoordinatesFromGoogleAsync(volunteer.Address);

          
                VolunteerManager.IsValidEmail(volunteer.Email);
            VolunteerManager.IsValidPhoneNumber(volunteer.Phone);
            VolunteerManager.IsValidId(volunteer.Id);

                var dalVolunteer = new DO.Volunteer
                {
                    idVol = volunteer.Id,
                    adress = volunteer.Address,
                    name = volunteer.FullName,
                    email = volunteer.Email,
                    phoneNumber = volunteer.Phone,
                    password = volunteer.Password,
                    latitude = 0,
                    longitude = 0,
                    limitDestenation = volunteer.MaxDistance ?? 0,
                    isActive = volunteer.IsActive,
                    role = (DO.Role)volunteer.Role,
                    distanceType = (DO.TypeDistance)volunteer.DistanceType
                };

                lock (AdminManager.BlMutex)
                {
                    _dal.volunteer.Create(dalVolunteer);
                }

                VolunteerManager.Observers.NotifyListUpdated();
                // חישוב קואורדינטות אסינכרוני (לא ממתינים!)
                _ = Task.Run(() => VolunteerManager.updateCoordinatesForVolunteerAsync(dalVolunteer));
            
        }
        catch (Exception ex)
        {
            throw new BlInvalidValueException("Failed to add volunteer.", ex);
        }
    }
   
    public void DeleteVolunteer(int volunteerId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        try
        {
            lock (AdminManager.BlMutex) // נעילה סביב גישה ל-DAL
            {
                _dal.volunteer.Delete(volunteerId);
            }
            VolunteerManager.Observers.NotifyListUpdated();
        }
        catch (Exception ex)
        {
            throw new BlDoesNotExistException($"Failed to delete volunteer with ID={volunteerId}.", ex);
        }
    }

    public BO.Volunteer GetVolunteerDetails(int volunteerId)
    {
        try
        {


            DO.Volunteer volunteerDO;
            lock (AdminManager.BlMutex)
                volunteerDO = _dal.volunteer.Read(volunteerId)?? throw new BlDoesNotExistException($"Volunteer with ID={volunteerId} does not exist.");
           

            return new BO.Volunteer
            {
                Id = volunteerId,
                FullName = volunteerDO.name,
                Phone = volunteerDO.phoneNumber,
                Email = volunteerDO.email,
                Address = volunteerDO.adress,
                IsActive = volunteerDO.isActive,
                Password = volunteerDO.password,
                Latitude = volunteerDO.latitude,
                Longitude = volunteerDO.longitude,
                MaxDistance = volunteerDO.limitDestenation,
                Role = (BO.Role?)volunteerDO.role

            };
        }
        catch (Exception ex)
        {
            throw new BlDoesNotExistException("Failed to retrieve volunteer details.", ex);
        }
    }
   

    public IEnumerable<VolunteerInList> GetVolunteersList(bool? isActive = null, VolunteerSortBy? sortBy = null)
    {
        try
        {
            IEnumerable<DO.Volunteer> volunteers;
            lock (AdminManager.BlMutex)
                  volunteers = _dal.volunteer.ReadAll();

            if (isActive.HasValue)
            {
                volunteers = volunteers.Where(v => v.isActive == isActive.Value);
            }

            if (sortBy.HasValue)
            {
                volunteers = sortBy switch
                {
                    VolunteerSortBy.Id => volunteers.OrderBy(v => v.idVol),
                    VolunteerSortBy.Name => volunteers.OrderBy(v => v.name),
                    VolunteerSortBy.ActivityStatus => volunteers.OrderBy(v => v.isActive),
                    _ => volunteers.OrderBy(v => v.idVol)
                };
            }
            else
            {
                volunteers = volunteers.OrderBy(v => v.idVol);
            }

            return volunteers.Select(v => new VolunteerInList()
            {
                Id = v.idVol,
                FullName = v.name,
                Phone = v.phoneNumber,
                mail = v.email,
                IsActive = v.isActive,
                CurrentCallId = _dal.assignment.ReadAll()
                .Count(a =>
                    a.volunteerId == v.idVol &&
                    a.assignKind != DO.Hamal.cancelByManager && // לא בוטל על ידי מנהל
                    a.assignKind != DO.Hamal.cancelByVolunteer // לא בוטל על ידי מתנדב
                )
            });

        }
        catch (Exception ex)
        {
            throw new BlInvalidValueException("Failed to retrieve volunteers list.", ex);
        }
    }

    public string Login(string username, string password)
    {
        int userId = int.Parse(username);
        DO.Volunteer vol;
        lock(AdminManager.BlMutex)
                vol = _dal.volunteer.Read(userId);

        if (vol == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }
        if (vol.password != password)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }
        return vol.role switch
        {
            DO.Role.Manager => "Manager",
            DO.Role.Volunteer => "Volunteer",
            _ => throw new BlInvalidValueException("Invalid role.")
        };
    }

    public void UpdateVolunteer(int requesterId, BO.Volunteer volunteer)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        try
        {
            VolunteerManager.UpdateVolunteer(requesterId, volunteer);
        }catch(Exception ex)
        {
            throw;
        }
        
       
    }

    public int GetVolunteerForCall(int callId)
    {
        lock (AdminManager.BlMutex)
          return  _dal.assignment.ReadAll()
            .Where(a => a.callId == callId)
            .Select(a => a.volunteerId)
             .FirstOrDefault(); // מחזיר את הערך הראשון או 0 אם אין תוצאות
       
    }


    public void AddObserver(Action listObserver) =>
        VolunteerManager.Observers.AddListObserver(listObserver);

    public void AddObserver(int id, Action observer) =>
        VolunteerManager.Observers.AddObserver(id, observer);

    public void RemoveObserver(Action listObserver) =>
        VolunteerManager.Observers.RemoveListObserver(listObserver);

    public void RemoveObserver(int id, Action observer) =>
        VolunteerManager.Observers.RemoveObserver(id, observer);

    public void SimulateVolunteers()
    {
        VolunteerManager.SimulateVolunteers();
    }
}

