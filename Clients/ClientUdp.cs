using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IPK_Proj1.Messages;

namespace IPK_Proj1.Clients
{
    public class ClientUdp : Client
    {
        private UdpClient UdpClient;

        private ushort MessageId;

        public ClientUdp(string serverIp, int port) : base(serverIp, port)
        {
            UdpClient = new UdpClient(serverIp, port);
            MessageId = 0;
        }

        public override void Connect()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override Task ListenForMessagesAsync()
        {
            throw new NotImplementedException();
        }

        public override string Receive()
        {
            throw new NotImplementedException();
        }

        public override async Task Send(IMessage message)
        {
            try
            {
                byte[] bytesString = message.ToUdpBytes(GetNextMessageId());

                // UdpClient.Send(bytesString, bytesString.Length, Server);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Vyskytla se chyba {ex.Message}");
            }
        }

        public override bool Connected()
        {
            throw new NotImplementedException();
        }

        protected override void HandleServerMessage(string message)
        {
            throw new NotImplementedException();
        }

        protected override void HandleReplyMessage(string status, string[] splittedMessage)
        {
            throw new NotImplementedException();
        }

        protected override void HandleChatMessage(string displayName, string[] splittedMessage)
        {
            throw new NotImplementedException();
        }

        protected override void HandleErrorMessage(string[] splittedMessage)
        {
            throw new NotImplementedException();
        }

        protected override void HandleByeMessage()
        {
            throw new NotImplementedException();
        }

        private ushort GetNextMessageId()
        {
            return ++MessageId;
        }
    }
}