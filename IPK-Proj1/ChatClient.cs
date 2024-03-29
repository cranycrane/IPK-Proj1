using IPK_Proj1.Clients;
using IPK_Proj1.Commands;
using IPK_Proj1.Exceptions;
using IPK_Proj1.Factory;
using IPK_Proj1.Messages;

namespace IPK_Proj1
{
    public class ChatClient
    {
        private Client Client;

        private readonly CommandFactory _commandFactory;

        private CommandLineSettings? Settings;

        private bool ExitHandled = false;
        
        private CancellationTokenSource _cts = new CancellationTokenSource();
        
        public ChatClient(CommandLineSettings? settings)
        {
            _commandFactory = new CommandFactory();
            Settings = settings;
            Client = CreateClient();
        }

        /// <summary>
        /// Starts the client - in while loop listens for messages, handling input and waiting for the end
        /// </summary>
        public async Task Start()
        {

            
            Logger.Debug("IPKChat-24 Version 1.0, write /help for more information");
            
            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true;
                await HandleExit();
            };
            
            while (true)
            {
                var listeningTask = Client!.ListenForMessagesAsync(_cts.Token);

                Logger.Debug("Nacitam vstup");

                
                string? input = Console.ReadLine();
                
                Logger.Debug("Docetl jsem vstup");

                if (input == null)
                {
                    await HandleExit();
                    break;
                }

                if (string.IsNullOrEmpty(input)) continue;

                if (input.StartsWith('/'))
                {
                    await HandleCommand(input);
                }
                else
                {
                    MessageCommand message = new MessageCommand();
                    await message.Execute(Client, [input]);
                }
            }
            
        }

        /// <summary>
        /// Handles gentle exit of the program.
        /// </summary>
        private async Task HandleExit()
        {
            if (ExitHandled)
            {
                return;
            }
            ExitHandled = true;

            if (Client.IsAuthenticated)
            {
                await Client.Send(new ByeMessage());
            }
            
            await Client.Disconnect();

            await _cts.CancelAsync();
            
            Logger.Debug("Konec aplikace");
            Environment.Exit(0);
        }
        
        /// <summary>
        /// Parses the input, creates appropriate command and executes it
        /// </summary>
        /// <param name="input">Input from stdin</param>
        private async Task HandleCommand(string input)
        {
            string[] splitInput = input.Substring(1).Split(' ');
            string commandName = splitInput[0];
            string[] parameters = splitInput.Skip(1).ToArray();

            try
            {
                ICommand command = _commandFactory.GetCommand(commandName);
                await command.Execute(Client, parameters);
            }
            catch (UnknownCommandException)
            {
                await Console.Error.WriteLineAsync("ERR: Zadany prikaz nenalezen, pouzijte /help");
            }
            catch (System.Exception e)
            {
                await Console.Error.WriteLineAsync(e.Message);
            }
        }
        

        /// <summary>
        /// Creates the client depending on the selected variant
        /// </summary>
        private Client CreateClient()
        {
            try
            {
                return Settings!.Protocol switch
                {
                    "tcp" => new ClientTcp(Settings.ServerIP, Settings.Port),
                    "udp" => new ClientUdp(Settings.ServerIP, Settings.Port, Settings.Timeout, Settings.Retries),
                    _ => throw new ArgumentException("Unsupported protocol")
                };
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException("Nepodařilo se vytvořit klienta", e);
            }
        }
    }
}