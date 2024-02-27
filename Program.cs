using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using IPK_Proj1.Clients;
using IPK_Proj1.Commands;
using IPK_Proj1.Factory;

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

        ChatClient chatClient = new ChatClient(settings);

        await chatClient.Start();
    }


}
