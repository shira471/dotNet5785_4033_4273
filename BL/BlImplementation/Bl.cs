using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlApi;
using DalApi;

namespace BlImplementation;

/// <summary>
/// המחלקה הראשית לשכבת ה-BL המממשת את הממשק הראשי IBl.
/// </summary>
internal class Bl : IBl
{
    public IVolunteer Volunteer { get; } = new VolunteerImplementation();
    public ICall Call { get; } = new CallImplementation();
    public IAdmin Admin { get; } = new AdminImplementation();

    
}
