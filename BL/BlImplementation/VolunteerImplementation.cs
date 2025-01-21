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

//using Newtonsoft.Json.Linq;
public class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.Idal _dal = DalApi.Factory.Get;

    public void AddVolunteer(BO.Volunteer volunteer)
    {
        if (volunteer == null)
        {
            throw new BlNullPropertyException("Volunteer object cannot be null.");
        }

        if (!IsValidEmail(volunteer.Email))
        {
            throw new BlInvalidValueException("Invalid email format.");
        }

        if (!IsValidPhoneNumber(volunteer.Phone))
        {
            throw new BlInvalidValueException("Invalid phone number.");
        }

        if (!IsValidId(volunteer.Id))
        {
            throw new BlInvalidValueException("Invalid ID number.");
        }

        if (volunteer.MaxDistance < 0)
            throw new ArgumentException("MaxDistance must be non-negative.");

        try
        {
            var temp = VolunteerManager.GetCoordinatesFromGoogle(volunteer.Address);
            IsValidEmail(volunteer.Email);
            IsValidPhoneNumber(volunteer.Phone);
            IsValidId(volunteer.Id);

            var dalVolunteer = new DO.Volunteer
            {
                idVol = volunteer.Id,
                adress = volunteer.Address,
                name = volunteer.FullName,
                email = volunteer.Email,
                phoneNumber = volunteer.Phone,
                password = volunteer.Password,
                latitude = temp[0],
                longitude = temp[1],
                limitDestenation = volunteer.MaxDistance ?? 0,
                isActive = volunteer.IsActive,
                role = (DO.Role)volunteer.Role,
                distanceType = (DO.TypeDistance)volunteer.DistanceType
            };
            _dal.volunteer.Create(dalVolunteer);
            VolunteerManager.Observers.NotifyListUpdated();
        }
        catch (Exception ex)
        {
            throw new BlInvalidValueException("Failed to add volunteer.", ex);
        }
    }

    public void DeleteVolunteer(int volunteerId)
    {
        try
        {
            _dal.volunteer.Delete(volunteerId);
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
            var volunteerDO = _dal.volunteer.Read(volunteerId);

            if (volunteerDO == null)
            {
                throw new BlDoesNotExistException($"Volunteer with ID={volunteerId} does not exist.");
            }

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
            var volunteers = _dal.volunteer.ReadAll();

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
                       .Count(a => a.volunteerId == v.idVol && a.finishTime == null)
            });

        }
        catch (Exception ex)
        {
            throw new BlInvalidValueException("Failed to retrieve volunteers list.", ex);
        }
    }

    public string Login(string username, string password)
    {
            int userId=int.Parse(username);
            var vol = _dal.volunteer.Read(userId);
          
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
        if (volunteer == null)
        {
            throw new BlNullPropertyException("Volunteer object cannot be null.");
        }

        //if (requesterId != volunteer.Id && !_dal.volunteer.Read(requesterId).role.Equals(DO.Role.Manager))
        //{
        //    throw new UnauthorizedAccessException("Only managers or the volunteer themselves can update details.");
        //}

        if (!IsValidEmail(volunteer.Email))
        {
            throw new BlInvalidValueException("Invalid email format.");
        }

        if (!IsValidPhoneNumber(volunteer.Phone))
        {
            throw new BlInvalidValueException("Invalid phone number.");
        }

        if (!IsValidId(volunteer.Id))
        {
            throw new BlInvalidValueException("Invalid ID number.");
        }

        var existingVolunteer = _dal.volunteer.Read(volunteer.Id)
            ?? throw new BlDoesNotExistException($"Volunteer with ID {volunteer.Id} not found.");

        //if (!object.Equals(existingVolunteer.role, volunteer.Role) &&
        //    !object.Equals(_dal.volunteer.Read(requesterId).role, DO.Role.Manager))
        //{
        //    throw new UnauthorizedAccessException("Only a manager can update the volunteer's role.");
        //}

        if (existingVolunteer.adress != volunteer.Address)
        {
            var coordinates = VolunteerManager.GetCoordinatesFromGoogle(volunteer.Address);
            if (coordinates == null)
            {
                throw new BlInvalidValueException("Invalid address provided.");
            }
        }

        var updatedVolunteer = new DO.Volunteer(
            idVol: volunteer.Id,
            adress: volunteer.Address,
            name: volunteer.FullName,
            email: volunteer.Email,
            phoneNumber: volunteer.Phone,
            password: volunteer.Password ?? existingVolunteer.password,
            latitude: volunteer.Latitude ?? existingVolunteer.latitude,
            longitude: volunteer.Longitude ?? existingVolunteer.longitude,
            limitDestenation: volunteer.MaxDistance ?? existingVolunteer.limitDestenation,
            isActive: volunteer.IsActive,
            role: (DO.Role?)volunteer.Role,
            distanceType: (DO.TypeDistance?)volunteer.DistanceType
        );

        try
        {
            _dal.volunteer.Update(updatedVolunteer);
            VolunteerManager.Observers.NotifyItemUpdated(updatedVolunteer.idVol);
            VolunteerManager.Observers.NotifyListUpdated();
        }
        catch (Exception ex)
        {
            throw new BlInvalidValueException("Failed to update the volunteer details.", ex);
        }
    }

    private bool IsValidEmail(string email)
    {
        
            var mail = new System.Net.Mail.MailAddress(email);
            return mail.Address == email;
    }

    private bool IsValidPhoneNumber(string phoneNumber)
    {
        return phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 7;
    }

    private bool IsValidId(int id)
    {
        return id > 0 && id.ToString().Length == 9;
    }
    public int GetVolunteerForCall(int callId)
    {
        return _dal.assignment.ReadAll()
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
}

