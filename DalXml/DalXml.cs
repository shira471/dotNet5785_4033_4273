using DalApi;
using DO;

namespace Dal;

public sealed class DalXml : Idal
{
    public Iassignment assignment { get; } = new AssignmentImplementation();

   // public Iassignment assignment => throw new NotImplementedException();

    public Icall call { get; } = new CallImplementation();

    //public Icall call => throw new NotImplementedException();

    public Ivolunteer volunteer { get; } = new VolunteerImplementation();

    //public Ivolunteer volunteer => throw new NotImplementedException();

    public Iconfig config { get; } = new ConfigImplementation();

    //public Iconfig config => throw new NotImplementedException();

    public void ResetDB()
    {
        assignment.DeleteAll();
        call.DeleteAll();
        volunteer.DeleteAll();
        config.Reset();
    }
}
