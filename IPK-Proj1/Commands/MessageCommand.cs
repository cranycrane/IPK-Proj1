using IPK_Proj1.Clients;
using IPK_Proj1.Messages;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IPK_Proj1.Commands
{
    public class MessageCommand : ICommand
    {
        public async Task Execute(Client client, string[] parameters)
        {
            ValidateArgs(parameters);

            if (!client.Connected())
            {
                client.Connect();
            }

            ChatMessage message = new ChatMessage(client.DisplayName!, parameters[0]);

            await client.Send(message);
        }

        public void ValidateArgs(string[] parameters)
        {
            string pattern = @"^[\x20-\x7E]*$";
            
            if (parameters[0].Length >= 1400 || !Regex.IsMatch(parameters[0], pattern))
            {
                throw new ArgumentException("ERR: Long message or unexpected characters");
            }
        }
    }
}