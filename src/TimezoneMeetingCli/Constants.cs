namespace TimezoneMeetingCli;

/// <summary>
/// Centralized constants outlining CLI naming conventions and shared UX constraints 
/// as bounded by the project constitution.
/// </summary>
public static class Constants
{
    // CLI Standards (Constitution Principle III)
    public const string CliStandardTimeLookupName = "time.lookup"; 
    public const string CliStandardTimeScheduleName = "time.schedule";

    // Application constraints
    public const int DefaultPerformanceStartupLimitMs = 200;
}