using IPK_Proj1.Messages;
using System.Text.RegularExpressions;

namespace IPK_Proj1.Clients
{
    public abstract class Client
    {
        protected string ServerIp { get; set; }
        protected int Port { get; set; }

        public string? DisplayName { get; set; }

        public bool IsAuthenticated { get; set; }

        public bool IsWaitingReply { get; set; }

        protected TaskCompletionSource<bool>? ReplyReceivedTcs;

        protected SemaphoreSlim ReplySemaphore = new SemaphoreSlim(1, 1);

        public Client(string serverIp, int serverPort)
        {
            ServerIp = serverIp;
            Port = serverPort;
            IsAuthenticated = false;
            ReplyReceivedTcs = null;
            IsWaitingReply = false;
        }

        /// <summary>
        /// Changes DisplayName of a client
        /// </summary>
        /// <param name="newName">New Displayname</param>
        /// <exception cref="ArgumentException">Thrown when is <paramref name="newName"/> wrong - too long or unexpected characters.</exception>
        public void ChangeDisplayName(string newName)
        {
            if (!Regex.IsMatch(newName, "^[\x20-\x7E]{1,20}$"))
            {
                throw new ArgumentException(
                    "ERR: Displayname must contain only printable characters and maximum of 20");
            }

            DisplayName = newName;
        }


        /// <summary>
        /// Connects client to server, usable only in TCP variant
        /// </summary>
        /// <exception cref="Exception">Thrown when could not connect to the server.</exception>
        public abstract void Connect();

        /// <summary>
        /// Sends message to server
        /// </summary>
        /// <param name="message">Message to send</param>
        public abstract Task Send(IMessage message);

        /// <summary>
        /// Listening for messages from server and handling them
        /// </summary>
        /// <param name="cancellationToken">Cancel token to end receiving while ending program</param>
        public abstract Task ListenForMessagesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Parses server message and handle the behaviour
        /// </summary>
        /// <param name="receivedBytes">Received message bytes</param>
        /// <param name="bytesRead">Length of the message</param>
        protected abstract Task HandleServerMessage(byte[] receivedBytes, int bytesRead);

        /// <summary>
        /// Handles behaviour of Reply message
        /// </summary>
        /// <param name="message">Reply message</param>
        protected async Task HandleReplyMessage(ReplyMessage message)
        {
            if (message.IsOk == "OK")
            {
                if (!IsAuthenticated)
                {
                    IsAuthenticated = true;
                }

                await Console.Error.WriteAsync($"Success: {message.Content}\n");
            }
            else if (message.IsOk == "NOK")
            {
                await Console.Error.WriteAsync($"Failure: {message.Content}\n");
            }
            else
            {
                throw new Exception($"ERR: Unexpected server status code '{message.IsOk}'");
            }

            if (message.Content.Contains("Authentication successful"))
            {
                IsAuthenticated = true;
            }

            await ReplySemaphore.WaitAsync();
            if (ReplyReceivedTcs != null)
            {
                Logger.Debug("ReplyTask OK");
                ReplyReceivedTcs.SetResult(true);
            }
            else
            {
                Logger.Debug("IS NULL!!!");
            }

            ReplySemaphore.Release();
        }

        /// <summary>
        /// Handles behaviour of MSG message
        /// </summary>
        /// <param name="message">MSG message</param>
        protected void HandleChatMessage(ChatMessage message)
        {
            Console.Write($"{message.DisplayName}: {message.Content}\n");
        }

        /// <summary>
        /// Handles behaviour of ERR message
        /// </summary>
        /// <param name="message">ERR message</param>
        protected async Task HandleErrorMessage(ErrorMessage message)
        {
            await Console.Error.WriteAsync($"ERR FROM {message.DisplayName}: {message.Content}\n");
        }

        /// <summary>
        /// Handles behaviour of BYE message
        /// </summary>
        protected abstract void HandleByeMessage();

        /// <summary>
        /// Disconnects the client from the server
        /// </summary>
        public abstract Task Disconnect();

        /// <summary>
        /// Handles behaviour of BYE message
        /// </summary>
        /// <returns>true if connected (always true in UDP variant)</returns>
        public abstract bool Connected();
    }
}