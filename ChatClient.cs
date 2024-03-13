using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IPK_Proj1.Clients;
using IPK_Proj1.Commands;
using IPK_Proj1.Exceptions;
using IPK_Proj1.Factory;
using IPK_Proj1.Messages;

namespace IPK_Proj1
{
    public class ChatClient
    {
        private Client client;

        private readonly CommandFactory commandFactory;
        

        public ChatClient(CommandLineSettings settings)
        {
            client = CreateClient(settings);
            commandFactory = new CommandFactory();
            
        }

        public async Task Start()
        {
            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true; // Zabrání ukončení procesu
                await client.Send(new ByeMessage());
                client.Disconnect(); // Řádné ukončení aplikace
                Logger.Debug("Koncim aplikaci");
            };
            
            Logger.Debug("IPKChat-24 Version 1.0, write /help for more information");

            var listeningTask = client.ListenForMessagesAsync();
            
            while (true)
            {
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input)) continue;

                if (input.StartsWith('/'))
                {
                    await HandleCommand(input);
                }
                else
                {
                    await SendMessage(input);
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
                await command.Execute(client, parameters);
            }
            catch (UnknownCommandException e)
            {
                await Console.Error.WriteLineAsync("Zadany prikaz nenalezen, pouzijte /help");
            }
            catch (System.Exception e)
            {
                await Console.Error.WriteLineAsync(e.Message);
            }
        }

        private async Task SendMessage(string input)
        {
            try
            {
                var message = new ChatMessage(client.DisplayName!, input);
                await client.Send(message);
                //Console.WriteLine($"{client.DisplayName}: {input}");
            }   
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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
