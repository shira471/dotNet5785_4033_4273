namespace DO;

public record Call
    (
    int id,
    string detail,
    string adress,
    double latitude,
    double longitude,
    Hamal? callType = null,
    DateTime? startTime = null,
    DateTime? maximumTime = null
    )
{
    public Call() : this(0, "", "", 0, 0) { }
}