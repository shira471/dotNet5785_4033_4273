namespace DO;

public record Assignment
    (
    int id,
    int callId,
    int volunteerId,
    DateTime? startTime = null,
    DateTime? finishTime = null,
    Hamal? assignKind = null
    
    )
{
    public Assignment() : this(0, 0, 0) { }
}