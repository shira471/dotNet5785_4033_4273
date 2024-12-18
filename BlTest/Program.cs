using System;
using System.Collections.Generic;
using BlApi;
using BO;
using BO.Enums;
using Helpers;

namespace VolunteerCallAssignment
{
    class Program
    {
        static void Main(string[] args)
        {
            IBl bl = BlApi.Factory.Get(); // Create an instance of the business logic layer

            Console.WriteLine("=== Volunteer Call Assignment System ===");

            while (true)
            {
                // Display the menu with options for the user
                Console.WriteLine("\nSelect an option:");
                Console.WriteLine("1. Add a new volunteer");
                Console.WriteLine("2. Add a new call");
                Console.WriteLine("3. Assign a call to a volunteer");
                Console.WriteLine("4. View volunteer details");
                Console.WriteLine("5. View call details");
                Console.WriteLine("6. List all open calls by volunteer");
                Console.WriteLine("7. List all closed calls by volunteer");
                Console.WriteLine("0. Exit");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();
                try
                {
                    // Handle the user choice and call the corresponding method
                    switch (choice)
                    {
                        case "1":
                            AddNewVolunteer(bl); // Add a new volunteer
                            break;
                        case "2":
                            AddNewCall(bl); // Add a new call
                            break;
                        case "3":
                            AssignCallToVolunteer(bl); // Assign a call to a volunteer
                            break;
                        case "4":
                            ViewVolunteerDetails(bl); // View volunteer details
                            break;
                        case "5":
                            GetCallDetails(bl); // View call details
                            break;
                        case "6":
                            ListOpenCallsByVolunteer(bl); // List all open calls by a specific volunteer
                            break;
                        case "7":
                            ListClosedCallsByVolunteer(bl); // List all closed calls by a specific volunteer
                            break;
                        case "0":
                            Console.WriteLine("Exiting system. Goodbye!"); // Exit the program
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Please try again."); // Handle invalid input
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Catch any exceptions and display an error message
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        // Method to add a new volunteer
        static void AddNewVolunteer(IBl bl)
        {
            Console.WriteLine("\n--- Add New Volunteer ---");
            Console.Write("Id: ");
            int id = int.Parse(Console.ReadLine()); // Get volunteer ID
            Console.Write("Name: ");
            string Name = Console.ReadLine(); // Get volunteer name
            Console.Write("Address: ");
            string address = Console.ReadLine(); // Get volunteer address
            Console.Write("Phone Number: ");
            string phone = Console.ReadLine(); // Get volunteer phone number
            Console.Write("Email: ");
            string email = Console.ReadLine(); // Get volunteer email
            Console.Write("Volunteer Status (true=Active, false=Inactive): ");
            var status = bool.Parse(Console.ReadLine()); // Get volunteer status (active/inactive)

            // Create a new Volunteer object and add it to the system
            var volunteer = new Volunteer(id, Name,phone, address, email, status) { };

            bl.Volunteer.AddVolunteer(volunteer); // Add the volunteer to the system via the business logic layer
            Console.WriteLine("Volunteer added successfully.");
        }

        // Method to add a new call
        static void AddNewCall(IBl bl)
        {
            Console.WriteLine("\n--- Add New Call ---");
            Console.Write("Description: ");
            string description = Console.ReadLine(); // Get call description
            Console.Write("Address: ");
            string address = Console.ReadLine(); // Get call address
            Console.Write("Latitude: ");
            double latitude = double.Parse(Console.ReadLine()); // Get call latitude
            Console.Write("Longitude: ");
            double longitude = double.Parse(Console.ReadLine()); // Get call longitude
            Console.Write("Call Type (0=Fire, 1=Medical, 2=Other): ");
            CallType callType = (CallType)int.Parse(Console.ReadLine()); // Get the call type (Fire, Medical, Other)
            Console.Write("Maximum End Time (yyyy-MM-dd HH:mm): ");
            DateTime maxEndTime = DateTime.Parse(Console.ReadLine()); // Get the call's maximum end time

            // Create a new Call object and add it to the system
            var call = new Call
            {
                Description = description,
                Address = address,
                Latitude = latitude,
                Longitude = longitude,
                CallType = callType,
                OpenTime = DateTime.Now, // Set the current time as the call's open time
                MaxEndTime = maxEndTime
            };

            bl.Call.AddCall(call); // Add the call to the system via the business logic layer
            Console.WriteLine("Call added successfully.");
        }

        // Method to assign a call to a volunteer
        static void AssignCallToVolunteer(IBl bl)
        {
            Console.WriteLine("\n--- Assign Call to Volunteer ---");
            Console.Write("Volunteer ID: ");
            int volunteerId = int.Parse(Console.ReadLine()); // Get volunteer ID
            Console.Write("Call ID: ");
            int callId = int.Parse(Console.ReadLine()); // Get call ID

            // Assign the call to the volunteer
            bl.Call.AssignCallToVolunteer(volunteerId, callId);
            Console.WriteLine("Call assigned successfully.");
        }

        // Method to view the details of a specific volunteer
        static void ViewVolunteerDetails(IBl bl)
        {
            Console.WriteLine("\n--- View Volunteer Details ---");
            Console.Write("Volunteer ID: ");
            int volunteerId = int.Parse(Console.ReadLine()); // Get volunteer ID

            // Get the volunteer details and display them
            var volunteer = bl.Volunteer.GetVolunteerDetails(volunteerId);
            Console.WriteLine($"\nVolunteer Details:\n{volunteer.Id}\n{volunteer.FullName}\n{volunteer.Phone}\n{volunteer.Address}");
        }

        // Method to view the details of a specific call
        static void GetCallDetails(IBl bl)
        {
            Console.WriteLine("\n--- Get Call Details ---");
            Console.Write("Call ID: ");
            int callId = int.Parse(Console.ReadLine()); // Get call ID

            // Get the call details and display them
            var call = bl.Call.GetCallDetails(callId);
            Console.WriteLine($"\nCall Details:\n{call}");
        }

        // Method to list all open calls for a specific volunteer
        static void ListOpenCallsByVolunteer(IBl bl)
        {
            Console.WriteLine("\n--- List Open Calls by Volunteer ---");
            Console.Write("Volunteer ID: ");
            int volunteerId = int.Parse(Console.ReadLine()); // Get volunteer ID

            // Get and display all open calls assigned to the volunteer
            var calls = bl.Call.GetOpenCallsByVolunteer(volunteerId, null, null);
            foreach (var call in calls)
            {
                Console.WriteLine(call); // Print each open call
            }
        }

        // Method to list all closed calls for a specific volunteer
        static void ListClosedCallsByVolunteer(IBl bl)
        {
            Console.WriteLine("\n--- List Closed Calls by Volunteer ---");
            Console.Write("Volunteer ID: ");
            int volunteerId = int.Parse(Console.ReadLine()); // Get volunteer ID

            // Get and display all closed calls assigned to the volunteer
            var calls = bl.Call.GetClosedCallsByVolunteer(volunteerId, null, null);
            foreach (var call in calls)
            {
                Console.WriteLine(call); // Print each closed call
            }
        }
    }
}
