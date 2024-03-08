using System.Net;
using System.Net.Sockets;
using System.Text;
using IPK_Proj1.Messages;
using System.Text.RegularExpressions;

namespace IPK_Proj1.Clients
{
    interface IClient
    {

        protected abstract void HandleReplyMessage();
        protected abstract void HandleChatMessage();
        protected abstract void HandleErrorMessage();
        protected abstract void HandleByeMessage();
    }
}
