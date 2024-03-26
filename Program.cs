
namespace IPK_Proj1;

class Program
{
    static async Task Main(string[] args)
    {
        ArgParser parser = new ArgParser();

        CommandLineSettings settings = parser.Parse(args);

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
            ChatClient chatClient = new ChatClient(settings);
            await chatClient.Start();
        }
        catch (InvalidOperationException e)
        {
            await Console.Error.WriteLineAsync($"ERR: {e.Message}");
            System.Environment.Exit(1);
        }

    }
}