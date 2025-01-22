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
using Newtonsoft.Json.Linq;
using PL.Calls;
using PL.viewModel;

namespace PL;

/// <summary>
/// Interaction logic for ActionSelectionWindow.xaml
/// </summary>
public partial class ActionSelectionManagerWindow : Window
{
    // Singleton instance of the business logic interface
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    // ViewModel instance for data binding
    public ActionManagerVM Vm
    {
        get { return (ActionManagerVM)GetValue(vMProperty); }
        set { SetValue(vMProperty, value); }
    }
    // Using a DependencyProperty as the backing store for Vm.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty vMProperty =
        DependencyProperty.Register("Vm", typeof(ActionManagerVM), typeof(ActionSelectionManagerWindow));
    public bool IsUpdate { get; private set; }
    public bool IsDelete { get; private set; }
    public bool IsCancel { get; private set; }
    public bool IsView { get; private set; }
    public string ActionMessage { get; set; }

    public ActionSelectionManagerWindow(string entityName)
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
    private void btnView_Click(object sender, RoutedEventArgs e)
    {
        IsView = true;
        DialogResult = true;
        Close();
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {

        IsCancel = true;
        DialogResult = true;
        Close();
    }
}
