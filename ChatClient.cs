using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IPK_Proj1.Clients;
using IPK_Proj1.Commands;
using IPK_Proj1.Factory;
using IPK_Proj1.Messages;

namespace IPK_Proj1
{
    public class ChatClient
    {
        private Client client;

        private ushort timeout;

        private byte retries;

        private readonly CommandFactory commandFactory;
        

        public ChatClient(CommandLineSettings settings)
        {
            client = CreateClient(settings);
            timeout = settings.Timeout;
            retries = settings.Retries;
            commandFactory = new CommandFactory();
            
        }

        public async Task Start()
        {
            Console.WriteLine("IPKChat-24 Version 1.0, write /help for more information");

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

            ICommand command = commandFactory.GetCommand(commandName);
            try
            {
                await command.Execute(client, parameters);
            }
            catch (Exception e)
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
            if (settings.Protocol == "tcp")
            {
                return new ClientTcp(settings.ServerIP, settings.Port);
            }
            else if (settings.Protocol == "udp")
            {
                return new ClientUdp(settings.ServerIP, settings.Port);
            }
            else
            {
                Console.Error.WriteLine("Neočekávaný protokol: " + settings.Protocol);
                throw new ArgumentException("Unsupported protocol");
            }
        }

    }


}
