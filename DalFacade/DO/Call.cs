
namespace DO;

public record Call
    (
    int id,
    Hamal callType,
    string detail,
    string adress,
    double latitude,
    double longitude,
    DateTime startTime,
    DateTime maximumTime
    )
{
}
