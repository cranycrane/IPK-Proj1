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
    public class AuthCommand : ICommand
    {
        public async Task Execute(Client client, string[] parameters)
        {
            
            ValidateArgs(parameters);

            string username = parameters[0];
            string secret = parameters[1];
            string displayName = parameters[2];
            
            client.ChangeDisplayName(displayName);

            if (!client.Connected())
            {
                client.Connect();
            }
            
            var message = new AuthMessage(username, secret, displayName);
            await client.Send(message);
            client.IsWaittingReply = true;
        }

        public void ValidateArgs(string[] parameters)
        {
            if (parameters.Length != 3)
            {
                throw new ArgumentException("Unexpected number of parameters in a command");
            }
            
            string username = parameters[0];
            string secret = parameters[1];
            string displayName = parameters[2];

            if (!Regex.IsMatch(username, "^[A-Za-z0-9]{1,20}$"))
            {
                throw new ArgumentException("Username must contain only A-Z, a-z, 0-9 and maximum of 20 characters");
            }

            if (!Regex.IsMatch(secret, "^[\x20-\x7E]{1,128}$"))
            {
                throw new ArgumentException("Secret must contain only printable characters and maximum of 128");
            }

            if (!Regex.IsMatch(displayName, "^[\x20-\x7E]{1,20}$"))
            {
                throw new ArgumentException("Displayname must contain only printable characters and maximum of 20");
            }
        }
    }
}
