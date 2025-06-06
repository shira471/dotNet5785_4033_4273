﻿using BO;
using DO;
using PL.viewModel;
using PL.Volunteer;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PL.Calls;

/// <summary>
/// Interaction logic for SelectCallWindow.xaml
/// </summary>
public partial class SelectCallWindow : Window
{
    // Singleton instance of the business logic interface
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private volatile DispatcherOperation? _callListUpdateOperation = null;

    // ViewModel instance for data binding
    public SelectCallWindowVM Vm
    {
        get { return (SelectCallWindowVM)GetValue(vMProperty); }
        set { SetValue(vMProperty, value); }
    }


    // Using a DependencyProperty as the backing store for Vm.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty vMProperty =
        DependencyProperty.Register("Vm", typeof(SelectCallWindowVM), typeof(SelectCallWindow));

    // Constructor for initializing the window with a specific volunteer ID
    public SelectCallWindow(int volunteerId)
    {
        InitializeComponent();
        try
        {
            Vm = new SelectCallWindowVM(volunteerId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void queryCallList()
    {
        try
        {
            Vm.LoadCalls();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void callListObserver()
    {
        if (_callListUpdateOperation is null || _callListUpdateOperation.Status == DispatcherOperationStatus.Completed)
        {
            _callListUpdateOperation = Dispatcher.BeginInvoke(() =>
            {
                queryCallList(); // עדכון הרשימה
            });
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl.Call.AddObserver(callListObserver); // הוסף את המשקיף
            queryCallList(); // טען את הרשימה הראשונית
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {

        try
        {
            s_bl?.Call.RemoveObserver(callListObserver);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error closing window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CallsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var selectedCall = Vm.Selected;
        if (selectedCall == null)
        {
            MessageBox.Show("No call selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show($"Do you want to handle call {selectedCall.Id}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                s_bl.Call.AssignCallToVolunteer(Vm.VolunteerId, selectedCall.Id);
                MessageBox.Show($"Call {selectedCall.Id} has been assigned to volunteer {Vm.VolunteerId}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
               
                queryCallList();
               
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning call to volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window is VolunteerWindow volunteerWindow) // מחפש את VolunteerWindow בלבד
            {
                volunteerWindow.Show();   // מבטיח שהחלון יהיה גלוי
                volunteerWindow.Activate(); // מביא אותו לקדמת המסך
                this.Close(); // סוגר את החלון הנוכחי
                return;
            }
        }

        // אם VolunteerWindow לא פתוח, ניתן לפתוח אותו מחדש
        var newVolunteerWindow = new VolunteerWindow();
        newVolunteerWindow.Show();
        this.Close();
    }

}