using IPK_Proj1.Clients;
using System.Text.RegularExpressions;

namespace IPK_Proj1.Commands
{
    public class RenameCommand : ICommand
    {
        public Task Execute(Client client, string[] parameters)
        {
            ValidateArgs(parameters);
            
            string displayName = parameters[0];

            client.ChangeDisplayName(displayName);
            
            Logger.Debug($"New display name successfully set to {client.DisplayName}\n");
            return Task.CompletedTask;
        }

        public void ValidateArgs(string[] parameters)
        {
            if (parameters.Length != 1)
            {
                throw new ArgumentException("ERR: Unexpected number of parameters in a command");
            }
            
            string username = parameters[0];

            if (!Regex.IsMatch(username, "^[A-Za-z0-9]{1,20}$"))
            {
                throw new ArgumentException("ERR: Username must contain only A-Z, a-z, 0-9 and maximum of 20 characters");
            }
        }
    }
}
