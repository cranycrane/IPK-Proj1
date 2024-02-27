﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using IPK_Proj1.Messages;
using System.Text.RegularExpressions;

namespace IPK_Proj1.Clients
{
    abstract public class Client
    {
        private string ServerIp { get; set; }
        private int Port { get; set; }

        public string? DisplayName { get; set; }

        public Client(string serverIp, int serverPort)
        {
            ServerIp = serverIp;
            Port = serverPort;
        }

        public void ChangeDisplayName(string newName)
        {
            if (!Regex.IsMatch(newName, "^[\x20-\x7E]{1,20}$"))
            {
                throw new ArgumentException("Displayname must contain only printable characters and maximum of 20");
            }

            DisplayName = newName;
        }
        

        public abstract void Connect();
        public abstract Task Send(IMessage message);

        public abstract Task ListenForMessagesAsync();

        
        public abstract string? Receive();
        public abstract void Disconnect();

        public abstract bool Connected();
        protected abstract void HandleServerMessage(string message);

        protected abstract void HandleReplyMessage(string status, string[] splittedMessage);
        protected abstract void HandleChatMessage(string displayName, string[] splittedMessage);
        protected abstract void HandleErrorMessage(string[] splittedMessage);
        protected abstract void HandleByeMessage();
    }
}