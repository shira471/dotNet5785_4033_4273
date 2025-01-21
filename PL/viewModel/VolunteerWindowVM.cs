using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PL.viewModel;

internal class VolunteerWindowVM : ViewModelBase
{
    private readonly BlApi.IBl _bl;
    private readonly int _volunteerId;

    public VolunteerWindowVM(int volunteerId)
    {
        _bl = BlApi.Factory.Get();
        _volunteerId = volunteerId;
        LoadVolunteerDetails();
        LoadAssignedCall();
    }

    private BO.Volunteer _currentVolunteer;
    public BO.Volunteer CurrentVolunteer
    {
        get => _currentVolunteer;
        set
        {
            _currentVolunteer = value;
            OnPropertyChanged(nameof(CurrentVolunteer));
        }
    }

    private BO.Call _assignedCall;
    public BO.Call AssignedCall
    {
        get => _assignedCall;
        set
        {
            _assignedCall = value;
            OnPropertyChanged(nameof(AssignedCall));
            OnPropertyChanged(nameof(IsCallAssigned));
            OnPropertyChanged(nameof(AssignedCallDetails));
        }
    }

    public bool IsCallAssigned => AssignedCall != null;

    public string AssignedCallDetails => AssignedCall != null
        ? $"Call ID: {AssignedCall.Id}, Description: {AssignedCall.Description}, Address: {AssignedCall.Address}, Open Time: {AssignedCall.OpenTime}"
        : "No active call assigned.";

    private void LoadVolunteerDetails()
    {
        try
        {
            CurrentVolunteer = _bl.Volunteer.GetVolunteerDetails(_volunteerId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading volunteer details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadAssignedCall()
    {
        try
        {
            AssignedCall = _bl.Call.GetAssignedCallByVolunteer(_volunteerId);
        }
        catch (Exception)
        {
            AssignedCall = null;
        }
    }

    public void EndAssignedCall()
    {
        if (AssignedCall == null) return;

        try
        {
            _bl.Call.CloseCallAssignment(_volunteerId, AssignedCall.Id);
            MessageBox.Show("The call has been successfully closed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAssignedCall();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error closing the call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void CancelAssignedCall()
    {
        if (AssignedCall == null) return;

        try
        {
            _bl.Call.CancelCallAssignment(_volunteerId, AssignedCall.Id);
            MessageBox.Show("The call assignment has been canceled.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAssignedCall();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error canceling the call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
