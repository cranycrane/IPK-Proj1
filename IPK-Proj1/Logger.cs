namespace IPK_Proj1;

public static class Logger
{
    public static bool IsDebugEnabled { get; set; } = false;

    public static void Debug(string message)
    {
        if (IsDebugEnabled)
        {
            Console.WriteLine("DEBUG: " + message);
        }
    }
}
