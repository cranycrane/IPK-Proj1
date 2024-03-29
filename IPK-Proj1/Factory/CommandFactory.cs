using IPK_Proj1.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPK_Proj1.Exceptions;

namespace IPK_Proj1.Factory
{
    public class CommandFactory
    {
        public ICommand GetCommand(string commandName)
        {
            switch (commandName.ToLower())
            {
                case "auth":
                    return new AuthCommand();
                case "help":
                    return new HelpCommand();
                case "join":
                    return new JoinCommand();
                case "rename":
                    return new RenameCommand();
                default:
                    throw new UnknownCommandException("Entered command not found");
            }
        }
    }

}
