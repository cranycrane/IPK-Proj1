using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
            MaxRetries = retries;
            MessageId = 0;
            ReceivedMessageIds = new List<ushort>();
            AckReceivedTcs = null;
            IsAck = false;
            Server = CreateIpEndPoint(Port);
            UdpClient = new UdpClient(0);
            Logger.Debug($"IP adresa serveru: {Server.Address}");
            Logger.Debug($"Port serveru: {Server.Port}");
        }

        private IPEndPoint CreateIpEndPoint(int port)
        {
            IPAddress? ipAddress;

            if (!IPAddress.TryParse(ServerIp, out ipAddress))
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ServerIp);

                if (hostEntry.AddressList.Length == 0)
                {
                    throw new ArgumentException("Nelze získat IP adresu z hostname.", nameof(ServerIp));
                }

                ipAddress = hostEntry.AddressList
                    .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

                if (ipAddress == null)
                {
                    throw new ArgumentException("Nenalezena žádná kompatibilní IPv4 adresa.", nameof(ServerIp));
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
            Disconnect();
        }

        public override void Disconnect()
        {
            UdpClient.Close();
            Environment.Exit(0);
        }

        public override async Task ListenForMessagesAsync()
        {
            while (true)
            {
                UdpReceiveResult result;
                try
                {
                    result = await UdpClient.ReceiveAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

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

        protected override async Task HandleServerMessage(byte[] receivedBytes, int bytesRead)
        {
            ushort messageId = BitConverter.ToUInt16(receivedBytes, 1);

            if (ReceivedMessageIds.Contains(messageId) && receivedBytes[0] != 0)
            {
                string content = Encoding.UTF8.GetString(receivedBytes.ToArray());
                Logger.Debug($"Already got this message: {content}");
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
                    await AckSemaphore.WaitAsync();
                    IsAck = true;
                    if (AckReceivedTcs != null)
                    {
                        AckReceivedTcs!.SetResult(true);
                    }
                    else
                    {
                        Logger.Debug("ACK IS NULL!!!");
                    }

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
                    ushort refMessageId = BitConverter.ToUInt16(receivedBytes, 4);
                    await HandleReplyMessage(new ReplyMessage(content, isOk, messageId, refMessageId));
                    break;
                }
                case 4:
                {
                    string displayName = ExtractDisplayName(receivedBytes, 3);
                    string content = Encoding.UTF8.GetString(receivedBytes.Skip(displayName.Length + 3).ToArray());
                    HandleChatMessage(new ChatMessage(displayName, content, messageId));
                    break;
                }
                case 254:
                {
                    string displayName = ExtractDisplayName(receivedBytes, 3);
                    string content = Encoding.UTF8.GetString(receivedBytes.Skip(displayName.Length + 3).ToArray());
                    await HandleErrorMessage(new ErrorMessage(displayName, content, messageId));
                    break;
                }
                case 255:
                {
                    Console.WriteLine("BYE");
                    HandleByeMessage();
                    break;
                }
                default:
                {
                    Console.Error.WriteLine("TUTO ZPRAVU JSEM NECEKAL");
                    break;
                }
            }
        }

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

        protected string ExtractDisplayName(byte[] receivedBytes, int startIndex)
        {
            // Nalezení indexu nulového bajtu, který ukončuje DisplayName
            int endIndexOfDisplayName = Array.IndexOf(receivedBytes, (byte)0, startIndex);

            if (endIndexOfDisplayName == -1)
            {
                throw new Exception("Nulový bajt ukončující DisplayName nebyl nalezen.");
            }

            // Výpočet délky DisplayName
            int displayNameLength = endIndexOfDisplayName - startIndex;

            // Extrahování bajtů DisplayName a jejich převod na řetězec
            string displayName = Encoding.UTF8.GetString(receivedBytes, startIndex, displayNameLength);

            return displayName;
        }


        public override async Task Send(IMessage message)
        {
            await AckSemaphore.WaitAsync();
            AckReceivedTcs = new TaskCompletionSource<bool>();
            AckSemaphore.Release();

            if (!IsAuthenticated && message.GetType() != typeof(AuthMessage))
            {
                await Console.Error.WriteAsync("ERR: Not authorized. Use /auth command\n");
                return;
            }

            if (message.IsAwaitingReply)
            {
                await ReplySemaphore.WaitAsync();
                ReplyReceivedTcs = new TaskCompletionSource<bool>();
                ReplySemaphore.Release();
            }

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

            if (!AckReceivedTcs!.Task.IsCompleted || !AckReceivedTcs.Task.Result)
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