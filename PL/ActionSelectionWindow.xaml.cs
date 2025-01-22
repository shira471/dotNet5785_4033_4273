using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL;

/// <summary>
/// Interaction logic for ActionSelectionWindow.xaml
/// </summary>
public partial class ActionSelectionWindow : Window
{
    public bool IsUpdate { get; private set; }
    public bool IsDelete { get; private set; }
    public bool IsCancel { get; private set; }
    public bool IsView { get; private set; }
    public bool IsViewCall { get; private set; }
    public string ActionMessage { get; set; }

    public ActionSelectionWindow(string entityName)
    {
        InitializeComponent();
        ActionMessage = $"Select an action for the {entityName}:";
        DataContext = this; // מגדיר את ה-DataContext כדי שה-Binding יעבוד
    }

    private void Update_Click(object sender, RoutedEventArgs e)
    {
        IsUpdate = true;
        DialogResult = true;
        Close();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        IsDelete = true;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        IsCancel = true;
        DialogResult = false; // קובע שהפעולה בוטלה
        Close();
    }
    private void View_Click(object sender, RoutedEventArgs e)
    {
        IsView = true;
        DialogResult = true; // קובע שהפעולה בוטלה
        Close();
    }
    private void ViewCall_Click(object sender, RoutedEventArgs e)
    {
        IsViewCall = true;
        DialogResult = true; // קובע שהפעולה בוטלה
        Close();
    }
}