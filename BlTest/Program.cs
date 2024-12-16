
using BlApi;
using BO.Enums;
using BlImplementation;
using BO;
namespace BlTest
{

    class Program
    {
        static void Main(string[] args)
        {
            IVolunteer volunteerManager = new VolunteerImplementation();
            ICall callManager = new CallImplementation();

            try
            {
                // יצירת מתנדב חדש
                Volunteer volunteer = new Volunteer(1, "יוסי כהן", "תל אביב", "yossi@example.com", true)
                {
                    Phone = "050-1234567",
                    Password = "password123",
                    Latitude = 32.0853,
                    Longitude = 34.7818,
                    MaxDistance = 10,
                    DistanceType = DistanceType.DrivingDistance
                };


                volunteerManager.AddVolunteer(volunteer);

                // קבלת פרטי המתנדב
                var volunteerDetails = volunteerManager.GetVolunteerDetails(1);
                Console.WriteLine($"Volunteer Details: {volunteerDetails.FullName}, {volunteerDetails.Address}, {volunteerDetails.Email}");

                // יצירת קריאה חדשה
                Call call = new Call
                {
                    Description = "עזרה עם אוכל",
                    Address = "תל אביב",
                    Latitude = 32.0853,
                    Longitude = 34.7818,
                    CallType = CallType.Emergency,
                    OpenTime = DateTime.Now,
                    MaxEndTime = DateTime.Now.AddHours(2)
                };

                callManager.AddCall(call);

                // קבלת פרטי קריאה
                var callDetails = callManager.GetCallDetails(1);
                Console.WriteLine($"Call Details: {callDetails.Description}, {callDetails.Address}, {callDetails.OpenTime}");

                // שיוך קריאה למתנדב
                callManager.AssignCallToVolunteer(1, 1);

                // סגירת קריאה
                callManager.CloseCallAssignment(1, 1);

                // מחיקת קריאה
                callManager.DeleteCall(1);

                // מחיקת מתנדב
                volunteerManager.DeleteVolunteer(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
