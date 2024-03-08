using IPK_Proj1.Clients;
using IPK_Proj1.Messages;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace IPK_Proj1.Commands
{
    public class RenameCommand : ICommand
    {
        public Task Execute(Client client, string[] parameters)
        {
            string displayName = parameters[0];

            client.ChangeDisplayName(displayName);
            
            Console.Write("New display name successfully set\n");
            return Task.CompletedTask;
        }

        public void ValidateArgs(string[] parameters)
        {
        }
    }
}
