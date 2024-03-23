using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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

        private readonly CommandFactory commandFactory;
        

        public ChatClient(CommandLineSettings settings)
        {
            Client = CreateClient(settings);
            commandFactory = new CommandFactory();
            
        }

        public async Task Start()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true; // Zabrání ukončení procesu
                if (Client.IsAuthenticated)
                {
                    await Client.Send(new ByeMessage());
                }
                Client.Disconnect(); // Řádné ukončení aplikace
                Logger.Debug("Koncim aplikaci");
                cts.Cancel(); // Signalizace zastavení
            };
            
            Logger.Debug("IPKChat-24 Version 1.0, write /help for more information");



            while (!cts.IsCancellationRequested)
            {
                var listeningTask = Client.ListenForMessagesAsync();
                
                string? input = Console.ReadLine();

                if (input == null) // Detekce EOF
                {
                    await Client.Send(new ByeMessage());
                    
                    Client.Disconnect();
                    Logger.Debug("EOF detekován, končím aplikaci.");
                    break; // Ukončení hlavní smyčky
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

        private async Task HandleCommand(string input)
        {


            string[] splitInput = input.Substring(1).Split(' ');
            string commandName = splitInput[0];
            string[] parameters = splitInput.Skip(1).ToArray();

            try
            {
                ICommand command = commandFactory.GetCommand(commandName);
                await command.Execute(Client, parameters);
            }
            catch (UnknownCommandException e)
            {
                await Console.Error.WriteLineAsync("ERR: Zadany prikaz nenalezen, pouzijte /help");
            }
            catch (System.Exception e)
            {
                await Console.Error.WriteLineAsync(e.Message);
            }
        }

        private Client CreateClient(CommandLineSettings settings)
        {
            try
            {
                return settings.Protocol switch
                {
                    "tcp" => new ClientTcp(settings.ServerIP, settings.Port),
                    "udp" => new ClientUdp(settings.ServerIP, settings.Port, settings.Timeout, settings.Retries),
                    _ => throw new ArgumentException("Unsupported protocol")
                };
            }
            catch (ArgumentException e)
            {
                //Console.Error.WriteLine($"ERR: {e.Message}");
                throw new InvalidOperationException("Nepodařilo se vytvořit klienta", e);
            }
        }

    }


}
