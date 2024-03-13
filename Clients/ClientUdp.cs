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
        private IPEndPoint? ServerAssignedPort = null;
        private ushort MessageId;
        private ushort Timeout;
        private byte MaxRetries;
        private bool IsAck;
        private int[] ReceivedMessageIds;

        public ClientUdp(string serverIp, int port, ushort timeout, byte retries) : base(serverIp, port)
        {
            Timeout = timeout;
            MaxRetries = retries;
            MessageId = 0;
            ReceivedMessageIds = [];
            IsAck = false;
            Server = CreateIpEndPoint(Port);
            UdpClient = new UdpClient(58452);
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
                        HandleServerMessage(receivedBytes, receivedBytes.Length);
                    }
                    catch (Exception e)
                    {
                        await Console.Error.WriteLineAsync($"ERR: {e.Message}");
                    }
                }
            }
        }

        protected override async void HandleServerMessage(byte[] receivedBytes, int bytesRead)
        {
            ushort messageId = BitConverter.ToUInt16(receivedBytes, 1);
            if (receivedBytes[0] != 0)
            {
                await SendConfirmMessage(messageId);
            }

            switch (receivedBytes[0])
            {
                case 0:
                {
                    IsAck = true;
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

                    IsWaittingReply = false;
                    Logger.Debug($"Got REPLY: {IsWaittingReply}");
                    string content = Encoding.UTF8.GetString(receivedBytes.Skip(6).ToArray());
                    ushort refMessageId = BitConverter.ToUInt16(receivedBytes, 4);
                    HandleReplyMessage(new ReplyMessage(content, isOk, messageId, refMessageId));
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
                    HandleErrorMessage(new ErrorMessage(displayName, content, messageId));
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
            Logger.Debug($"IsWaittingReply: {IsWaittingReply}");
            
            if (IsWaittingReply)
            {
                Logger.Debug("CANT SEND, WAITTING");
                return;
            }
            IsWaittingReply = message.IsAwaitingReply;
            
            Byte[] data = message.ToUdpBytes(GetNextMessageId());
            byte retryCount = 0;
            
            try
            {
                while (retryCount < MaxRetries && !IsAck)
                {
                    await UdpClient.SendAsync(data, data.Length, Server);
                    await Task.Delay(Timeout);
                    //await WaitForAck();

                    if (!IsAck)
                    {
                        retryCount++;
                        Logger.Debug(
                            $"Zprava nebyla potvrzena, opakuji pokus {retryCount} z {MaxRetries}");
                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"ERR: {ex.Message}");
            }

            if (!IsAck)
            {
                await Console.Error.WriteLineAsync("ERR: Vycerpany vsechny pokusy odeslani.");
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