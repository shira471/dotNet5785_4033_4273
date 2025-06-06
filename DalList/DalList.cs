﻿
namespace Dal;
using DalApi;

sealed internal class Dallist : Idal
{
    public static Idal Instance { get; } = new Dallist();
    private Dallist() { }
    public Ivolunteer volunteer { get; } = new VolunteerImplementation();

    public Icall call { get; } = new CallImplementation();

    public Iassignment assignment { get; } = new AssignmentImplementation();

    public Iconfig config { get; } = new ConfigImplementation();

    public object Config => throw new NotImplementedException();

    public void ResetDB()
    {
        volunteer.DeleteAll();
        call.DeleteAll();
        assignment.DeleteAll();
        config.Reset();
    }
}