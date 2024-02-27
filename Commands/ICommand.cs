using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPK_Proj1.Clients;

namespace IPK_Proj1.Commands
{
    public interface ICommand
    {
        public Task Execute(Client client, string[] parameters);

        protected void ValidateArgs(string[] parameters);
    }
}
