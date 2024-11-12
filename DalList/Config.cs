
namespace Dal;

internal static class Config
{
    internal const int nextCallId=0;
    internal const int nextAssinmentId=0;
    internal static DateTime clock { set; get; }=DateTime.Now;
    internal static TimeSpan riskRange { set; get; }=TimeSpan.Zero;
}
