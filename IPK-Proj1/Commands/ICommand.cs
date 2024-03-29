using IPK_Proj1.Clients;

namespace IPK_Proj1.Commands
{
    public interface ICommand
    {

        /// <summary>
        /// Executes a command
        /// </summary>
        /// <param name="client">Client</param>
        /// <param name="parameters">Array of parameters</param>
        public Task Execute(Client client, string[] parameters);

        /// <summary>
        /// Validates parameters in command
        /// </summary>
        /// <param name="parameters">Array of parameters</param>
        /// <exception cref="ArgumentException">Thrown when are wrong <paramref name="parameters"/> - unexpected count, unexpected format, length, etc.</exception>
        protected void ValidateArgs(string[] parameters);
    }
}
