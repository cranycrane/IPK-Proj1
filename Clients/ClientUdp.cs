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
        private IPEndPoint ServerAssignedPort;
        private ushort MessageId;
        private ushort Timeout;
        private byte MaxRetries;
        private bool IsAck;
        private int[] ReceivedMessageIds;

        public ClientUdp(string serverIp, int port, ushort timeout, byte retries) : base(serverIp, port)
        {
            Timeout = timeout;
            MaxRetries = retries;
            // MaxRetries = retries;
            MessageId = 0;
            ReceivedMessageIds = [];
            IsAck = false;
            Server = CreateIpEndPoint(Port);
            ServerAssignedPort = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine($"IP adresa serveru: {Server.Address}");
            Console.WriteLine($"Port serveru: {Server.Port}");
            UdpClient = new UdpClient(58452);

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

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }
        
        public override async Task ListenForMessagesAsync()
        {
            Console.WriteLine("PRIJIMAM ZPRAVY SERVERU");
            while (true) // nebo dokud není spojení ukončeno
            {
                UdpReceiveResult result;
                try
                {
                    result = await UdpClient.ReceiveAsync(); // Přijme datagram z jakéhokoli zdroje a portu
                }
                catch (ObjectDisposedException)
                {
                    // UdpClient byl uzavřen, ukončíme smyčku
                    break;
                }

                byte[] receivedBytes = result.Buffer;
                
                IPEndPoint senderEndPoint = result.RemoteEndPoint; // Získáme informace o odesílateli

                HandleServerMessage(receivedBytes[0], receivedBytes.Skip(1).ToArray());

                // Zde můžete přidat logiku pro zpracování zprávy, například potvrzení přijetí zprávy, pokud je to potřeba
            }
            Console.WriteLine("KONCIME SMYCKU");
        }

        protected void HandleServerMessage(byte messageCode, byte[] messageBytes)
        {

            switch (messageCode)
            {
                case 0x00:
                    IsAck = true;
                    break;
                case 0x01:
                    Console.WriteLine("REPLY");
                    break;
                case 0x04:
                    Console.WriteLine("MESSAGE");
                    break;
                case 0xFE:
                    Console.WriteLine("ERROR");
                    break;
                case 0xFF:
                    Console.WriteLine("BYE");
                    break;
                default:
                    Console.Error.WriteLine("TUTO ZPRAVU JSEM NECEKAL");
                    break;
            }
        }

        public override async Task Send(IMessage message)
        {
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
                        await Console.Error.WriteLineAsync($"Zprava nebyla potvrzena, opakuji pokus {retryCount} z {MaxRetries}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Vyskytla se chyba: {ex.Message}");
            }

            if (IsAck)
            {
                Console.WriteLine("Zprava byla potvrzena serverem");
            }
            else
            {
                await Console.Error.WriteLineAsync("Chyba: Vycerpany vsechny pokusy odeslani.");
            }

            IsAck = false;
        }

        public override bool Connected()
        {
            return true;
        }

        protected void HandleReplyMessage(byte[] message)
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