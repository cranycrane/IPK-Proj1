
namespace IPK_Proj1;

class Program
{
    private static ChatClient? _chatClient;
    private static CommandLineSettings? _settings;
    private static TaskCompletionSource<bool> _initializationComplete = new TaskCompletionSource<bool>();
    static async Task Main(string[] args)
    {
        InitializeAsync(args);

        await _initializationComplete.Task;

        if (_settings!.ShowHelp)
        {
            return;
        }

        if (_settings.IsDebugEnabled)
        {
            Logger.IsDebugEnabled = true;
        }
        
        try
        {
            await _chatClient!.Start();
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

        _settings = parser.Parse(args);
        
        _chatClient = new ChatClient(_settings);
        
        _initializationComplete.SetResult(true);
    }
}