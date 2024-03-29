
namespace IPK_Proj1;

class Program
{
    private static ChatClient chatClient;
    private static CommandLineSettings settings;
    private static TaskCompletionSource<bool> _initializationComplete = new TaskCompletionSource<bool>();
    static async Task Main(string[] args)
    {
        InitializeAsync(args);

        await _initializationComplete.Task;

        if (settings.ShowHelp)
        {
            return;
        }

        if (settings.IsDebugEnabled)
        {
            Logger.IsDebugEnabled = true;
        }
        
        try
        {
            await chatClient.Start();
        }
        catch (InvalidOperationException e)
        {
            await Console.Error.WriteLineAsync($"ERR: {e.Message}");
            System.Environment.Exit(1);
        }

    }
    
    private static void InitializeAsync(string[] args)
    {
        ArgParser parser = new ArgParser();

        settings = parser.Parse(args);
        
        chatClient = new ChatClient(settings);
        
        _initializationComplete.SetResult(true);
    }
}