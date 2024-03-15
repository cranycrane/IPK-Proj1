﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using IPK_Proj1.Messages;
using System.Text.RegularExpressions;

namespace IPK_Proj1.Clients
{
    abstract public class Client
    {
        protected string ServerIp { get; set; }
        protected int Port { get; set; }

        public string? DisplayName { get; set; }
        
        public bool IsAuthenticated { get; set; }
        
        public bool IsWaittingReply { get; set; }

        public Client(string serverIp, int serverPort)
        {
            ServerIp = serverIp;
            Port = serverPort;
            IsAuthenticated = false;
            IsWaittingReply = false;
        }

        public void ChangeDisplayName(string newName)
        {
            if (!Regex.IsMatch(newName, "^[\x20-\x7E]{1,20}$"))
            {
                throw new ArgumentException("ERR: Displayname must contain only printable characters and maximum of 20");
            }

            DisplayName = newName;
        }
        

        public abstract void Connect();
        public abstract Task Send(IMessage message);

        public abstract Task ListenForMessagesAsync();

        protected abstract void HandleServerMessage(byte[] receivedBytes, int bytesRead);

        protected void HandleReplyMessage(ReplyMessage message)
        {
            if (message.IsOk == "OK")
            {
                Console.Error.Write($"Success: {message.Content}\n");
            }
            else if (message.IsOk == "NOK")
            {
                Console.Error.Write($"Failure: {message.Content}\n");
            }
            else
            {
                throw new Exception($"ERR: Unexpected server status code '{message.IsOk}'");
            }
        }

        protected void HandleChatMessage(ChatMessage message)
        {
            Console.Write($"{message.DisplayName}: {message.Content}\n");
        }

        protected void HandleErrorMessage(ErrorMessage message)
        {
            Console.Error.Write($"ERR FROM {message.DisplayName}: {message.Content}\n");
            Disconnect();
        }
        
        protected abstract void HandleByeMessage();

        
        public abstract void Disconnect();

        public abstract bool Connected();
   
        
    }
}
