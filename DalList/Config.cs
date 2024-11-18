namespace DO;

public static class Config
{
    internal const int nextCallId = 0;
    private static int nextAssinmentId = 0;
    internal static int GetNextAssignId { get { return nextAssinmentId++; } }
    internal static DateTime clock { set; get; } = DateTime.Now;
    internal static void Reset()
    {
        nextAssinmentId = GetNextAssignId;
        clock = DateTime.Now;
    }
    internal static TimeSpan riskRange { set; get; } = TimeSpan.Zero;
}