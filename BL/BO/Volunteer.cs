using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BO
{
    public enum Role
    {
        Manager,
        volunteer
    }
    public enum distanceType
    {
        Air,
        Walking,
        Driving
    }
    internal class Volunteer
    {
        int Id;
        string FullName;
        string PhoneNumber;
        string Email;
        string? password=null;
        string? CurrentAddress=null;
        double?Latitude=null;
        double?Longitude=null;
        Role VolunteerRole;
        Boolean IsActive;
        distanceType DistanceCalculationType;
        double? MaxDistance=null;
        int TotalHandledCalls;
        int TotalCancelledCalls;
        int? TotalExpiredCalls = null;
        CallProgress CurrentCall;
        public Volunteer(int id, string fullName, string phoneNumber, string email, Role role)
        {
            if (!IsValidId(id)) throw new ArgumentException("Invalid ID.");
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name cannot be empty.");
            if (!IsValidPhoneNumber(phoneNumber)) throw new ArgumentException("Invalid phone number.");
            if (!IsValidEmail(email)) throw new ArgumentException("Invalid email format.");

            Id = id;
            FullName = fullName;
            PhoneNumber = phoneNumber;
            email = Email;
            VolunteerRole = role;
            IsActive = true; // Default to active
            DistanceCalculationType =distanceType.Air; // Default distance type
        }
        // Validation Methods
        private bool IsValidId(int id) => id > 0;

        private bool IsValidPhoneNumber(string phoneNumber) =>
            Regex.IsMatch(phoneNumber, @"^\+?\d{10,15}$");

        private bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private bool IsStrongPassword(string password) =>
            password.Length >= 8 &&
            Regex.IsMatch(password, @"[A-Z]") && // At least one uppercase
            Regex.IsMatch(password, @"[a-z]") && // At least one lowercase
            Regex.IsMatch(password, @"\d") &&    // At least one digit
            Regex.IsMatch(password, @"[@$!%*?&]"); // At least one special character

        // Methods
        public void UpdateAddress(string address, double? latitude, double? longitude)
        {
            if (string.IsNullOrWhiteSpace(address) || latitude == null || longitude == null)
                throw new ArgumentException("Invalid address or coordinates.");

            CurrentAddress = address;
            Latitude = latitude;
            Longitude = longitude;
        }

        public void IncrementHandledCalls() => TotalHandledCalls++;
        public void IncrementCancelledCalls() => TotalCancelledCalls++;
        public void IncrementExpiredCalls() => TotalExpiredCalls++;
    }

    // Placeholder for CallInProgress entity
    public class CallProgress
    {
        public int CallId { get; set; }
        public string Details { get; set; }
    }
}

