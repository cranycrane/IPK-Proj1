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
    public class HelpCommand : ICommand
    {
        public Task Execute(Client client, string[] parameters)
        {   
            ValidateArgs(parameters);
            Console.WriteLine("This is a help command");
            return Task.CompletedTask;
        }

        public void ValidateArgs(string[] parameters)
        {
            if (parameters.Length != 0)
            {
                throw new ArgumentException("ERR: Unexpected number of parameters in a command");
            }
        }
    }
}
