using System.Runtime.CompilerServices;

namespace DO;

public static class Config
{
    internal static int nextCallId { get; }
    private static int nextAssinmentId = 0;
    internal static int GetNextAssignId { get { return nextAssinmentId++; } }
    public static DateTime clock { set; get; } = DateTime.Now;
    [MethodImpl(MethodImplOptions.Synchronized)]

    internal static void Reset()
    {
        nextAssinmentId = 0;
        clock = DateTime.Now;
    }
    internal static TimeSpan riskRange { set; get; } = TimeSpan.Zero;
}