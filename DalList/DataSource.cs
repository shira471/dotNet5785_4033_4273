
namespace Dal;

internal static class DataSource
{
    internal static List<DO.Volunteer> volunteers { get; } = new();
    internal static List<DO.Call> calls { get; } = new();
    internal static List<DO.Assignment> assignments { get; } = new();
}
