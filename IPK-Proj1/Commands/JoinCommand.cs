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
    public class JoinCommand : ICommand
    {
        public async Task Execute(Client client, string[] parameters)
        {
            ValidateArgs(parameters);

            string channelId = parameters[0];

            if (!client.Connected())
            {
                client.Connect();
            }

            if (client.DisplayName == null)
            {
                throw new Exception("ERR: Client is not authenticated, use /auth command");
            }

            JoinMessage message = new JoinMessage(channelId, client.DisplayName);

            await client.Send(message);
            
        }

        public void ValidateArgs(string[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new ArgumentException("ERR: Unexpected number of parameters in a command");
            }
            
            string channelId = parameters[0];

            if (!Regex.IsMatch(channelId, @"^[A-Za-z0-9\.-]{1,20}$"))
            {
                throw new ArgumentException("ERR: Channel must contain only A-Z, a-z, 0-9 and maximum of 20 characters");
            }
        }
    }
}
