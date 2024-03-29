using System.Net;
using System.Net.Sockets;
using System.Text;
using IPK_Proj1.Messages;

namespace IPK_Proj1.Clients
{
    public class ClientUdp : Client
    {
        private UdpClient UdpClient;
        public IPEndPoint Server { get; set; }
        private ushort MessageId;
        private ushort Timeout;
        private byte MaxRetries;
        private bool IsAck { get; set; }
        protected SemaphoreSlim AckSemaphore = new SemaphoreSlim(1, 1);
        public TaskCompletionSource<bool>? AckReceivedTcs;
        private List<ushort> ReceivedMessageIds;

        public ClientUdp(string serverIp, int port, ushort timeout, byte retries) : base(serverIp, port)
        {
            Timeout = timeout;
            retries++;
            MaxRetries = retries;
            MessageId = 0;
            ReceivedMessageIds = new List<ushort>();
            AckReceivedTcs = null;
            IsAck = false;
            Server = CreateIpEndPoint(Port);
            Logger.Debug($"IP adresa serveru: {Server.Address}");
            Logger.Debug($"Port serveru: {Server.Port}");
            UdpClient = new UdpClient(0);
        }

        /// <summary>
        /// Gets IPAddress of the server
        /// </summary>
        /// <param name="port">Port of the server</param>
        /// <returns>IPEndpoint with the host IP adress</returns>
        /// <exception cref="ArgumentException">When could not get the IP adress</exception>
        private IPEndPoint CreateIpEndPoint(int port)
        {
            IPAddress? ipAddress;

            if (!IPAddress.TryParse(ServerIp, out ipAddress))
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ServerIp);

                if (hostEntry.AddressList.Length == 0)
                {
                    throw new ArgumentException("Could not get IP from the hostname.", nameof(ServerIp));
                }

                ipAddress = hostEntry.AddressList
                    .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

                if (ipAddress == null)
                {
                    throw new ArgumentException("Did not found compatible IPv4 address.", nameof(ServerIp));
                }
            }

            return new IPEndPoint(ipAddress, port);
        }

        public override void Connect()
        {
            throw new NotImplementedException();
        }

        protected override void HandleByeMessage()
        {
            UdpClient.Close();
            System.Environment.Exit(0);
        }

        public override Task Disconnect()
        {
            UdpClient.Close();
            return Task.CompletedTask;
        }

        public override async Task ListenForMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await UdpClient.ReceiveAsync(cancellationToken);

                    byte[] receivedBytes = result.Buffer;

                    Server = result.RemoteEndPoint;

                    if (receivedBytes.Length > 0)
                    {
                        try
                        {
                            await HandleServerMessage(receivedBytes, receivedBytes.Length);
                        }
                        catch (Exception e)
                        {
                            await Console.Error.WriteLineAsync($"ERR: {e.Message}");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Debug("Ended receiving messages");
            }
            catch (ObjectDisposedException)
            {
            }
        }

        protected override async Task HandleServerMessage(byte[] receivedBytes, int bytesRead)
        {
            byte[] messageIdBytes = receivedBytes.Skip(1).Take(2).ToArray();
            ushort messageId = BitConverter.ToUInt16(messageIdBytes, 0);
            Logger.Debug($"Got message with code: {receivedBytes[0]}");
            if (ReceivedMessageIds.Contains(messageId) && receivedBytes[0] != 0)
            {
                string content = Encoding.UTF8.GetString(receivedBytes.ToArray());
                Logger.Debug($"Already got this message: {messageId} - {content}");
                return;
            }

            if (receivedBytes[0] != 0)
            {
                Logger.Debug($"Sending ACK RefID: {messageId}");
                ReceivedMessageIds.Add(messageId);
                await SendConfirmMessage(messageId);
            }


            switch (receivedBytes[0])
            {
                case 0:
                {
                    Logger.Debug("Got ACK");
                    await AckSemaphore.WaitAsync();
                    IsAck = true;
                    AckReceivedTcs!.TrySetResult(true);

                    AckSemaphore.Release();
                    break;
                }
                case 1:
                {
                    string isOk;
                    if (receivedBytes[3] == 1)
                    {
                        isOk = "OK";
                    }
                    else
                    {
                        isOk = "NOK";
                    }

                    string content = Encoding.UTF8.GetString(receivedBytes.Skip(6).ToArray());
                    string cleanContent = CleanContent(content);
                    ushort refMessageId = BitConverter.ToUInt16(receivedBytes, 4);
                    await HandleReplyMessage(new ReplyMessage(cleanContent, isOk, messageId, refMessageId));
                    break;
                }
                case 4:
                {
                    string displayName = ExtractDisplayName(receivedBytes, 3);
                    string content = Encoding.UTF8.GetString(receivedBytes.Skip(displayName.Length + 3).ToArray());
                    string cleanContent = CleanContent(content);
                    HandleChatMessage(new ChatMessage(displayName, cleanContent, messageId));
                    break;
                }
                case 254:
                {
                    string displayName = ExtractDisplayName(receivedBytes, 3);
                    string content = Encoding.UTF8.GetString(receivedBytes.Skip(displayName.Length + 3).ToArray());
                    string cleanContent = CleanContent(content);
                    await HandleErrorMessage(new ErrorMessage(displayName, cleanContent, messageId));
                    Logger.Debug("Sending BYE to ERR message");
                    await Send(new ByeMessage());
                    await Disconnect();
                    System.Environment.Exit(1);
                    break;
                }
                case 255:
                {
                    Logger.Debug("BYE");
                    HandleByeMessage();
                    break;
                }
                default:
                {
                    await Console.Error.WriteLineAsync($"ERR: Unexpected server message with code {receivedBytes[0]}");
                    Logger.Debug("Sending ERR Message   ");
                    await Send(new ErrorMessage(DisplayName!, "Unexpected message code"));
                    await Send(new ByeMessage());
                    await Disconnect();
                    System.Environment.Exit(1);
                    break;
                }
            }
        }

        /// <summary>
        /// Cleans message content from unexpected bytes and characters
        /// </summary>
        /// <param name="content">Message content</param>
        /// <returns>Cleaned content</returns>
        protected string CleanContent(string content)
        {
            string cleanString = content.Replace("\0", string.Empty);
            cleanString = cleanString.Replace("\u0002", string.Empty);
            return cleanString;
        }

        /// <summary>
        /// Sends CONFIRM message to the server
        /// </summary>
        /// <param name="refMessageId">Reference message ID client is sending CONFIRM to</param>
        protected async Task SendConfirmMessage(ushort refMessageId)
        {
            Byte[] data = new ConfirmMessage().ToUdpBytes(refMessageId);
            try
            {
                await UdpClient.SendAsync(data, data.Length, Server);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERR: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses DisplayName from the message
        /// </summary>
        /// <param name="receivedBytes">Array of bytes from the message</param>
        /// <param name="startIndex">Start index, where displayName begins</param>
        /// <returns>Extracted displayName</returns>
        protected string ExtractDisplayName(byte[] receivedBytes, int startIndex)
        {
            int endIndexOfDisplayName = Array.IndexOf(receivedBytes, (byte)0, startIndex);

            if (endIndexOfDisplayName == -1)
            {
                throw new Exception("Nulový bajt ukončující DisplayName nebyl nalezen.");
            }

            int displayNameLength = endIndexOfDisplayName - startIndex;

            string displayName = Encoding.UTF8.GetString(receivedBytes, startIndex, displayNameLength);

            return displayName;
        }


        public override async Task Send(IMessage message)
        {
            if (!IsAuthenticated && message.GetType() != typeof(AuthMessage) && message.GetType() != typeof(ByeMessage))
            {
                await Console.Error.WriteAsync("ERR: Not authorized. Use /auth command\n");
                return;
            }

            await AckSemaphore.WaitAsync();
            AckReceivedTcs = new TaskCompletionSource<bool>();
            AckSemaphore.Release();

            if (message.IsAwaitingReply)
            {
                await ReplySemaphore.WaitAsync();
                ReplyReceivedTcs = new TaskCompletionSource<bool>();
                ReplySemaphore.Release();
            }

            Logger.Debug($"Sending message with ID: {MessageId}");
            Byte[] data = message.ToUdpBytes(GetNextMessageId());
            byte retryCount = 0;

            try
            {
                while (retryCount < MaxRetries && !AckReceivedTcs.Task.IsCompleted)
                {
                    await UdpClient.SendAsync(data, data.Length, Server);
                    await Task.Delay(Timeout);

                    await AckSemaphore.WaitAsync();
                    if (IsAck)
                    {
                        AckSemaphore.Release();
                        break;
                    }

                    AckSemaphore.Release();

                    retryCount++;
                    Logger.Debug($"Zprava nebyla potvrzena, opakuji pokus {retryCount} z {MaxRetries}");
                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"ERR: {ex.Message}");
            }

            if (!AckReceivedTcs!.Task.IsCompleted || !AckReceivedTcs.Task.Result || !IsAck)
            {
                await Console.Error.WriteLineAsync("ERR: Vycerpany vsechny pokusy odeslani.");
            }

            if (message.IsAwaitingReply)
            {
                await ReplyReceivedTcs!.Task;
            }

            IsAck = false;
        }

        public override bool Connected()
        {
            return true;
        }


        private ushort GetNextMessageId()
        {
            return MessageId++;
        }
    }
}