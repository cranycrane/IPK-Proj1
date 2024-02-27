using System.Net.Sockets;
using System.Text;
using IPK_Proj1.Messages;

namespace IPK_Proj1.Clients
{
    public class ClientTcp : Client
    {
        private TcpClient tcpClient;
        private NetworkStream? networkStream;
        private StreamReader reader;
        private StreamWriter writer;

        public ClientTcp(string serverIp, int port) : base(serverIp, port)
        {
            tcpClient = new TcpClient(serverIp, port);
            networkStream = tcpClient.GetStream();
            reader = new StreamReader(networkStream, Encoding.UTF8);
            writer = new StreamWriter(networkStream, Encoding.UTF8);
        }

        public override void Connect()
        {
            try
            {
                /*
                tcpClient.Connect(Server);
                networkStream = tcpClient.GetStream();
                writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };
                reader = new StreamReader(networkStream, Encoding.UTF8);
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
                // Handle exceptions (e.g., server not available)
            }
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override async Task Send(IMessage message)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message.ToTcpString());
            await networkStream!.WriteAsync(data, 0, data.Length);
        }

        public override string? Receive()
        {
            return reader.ReadLine();
        }

        public override bool Connected()
        {
            if (tcpClient.Connected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override async Task ListenForMessagesAsync()
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (networkStream != null)
                {
                    var bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        try
                        {
                            HandleServerMessage(message);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine(e.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Chyba při příjmu zprávy: {ex.Message}");
            }
        }


        protected override void HandleServerMessage(string message)
        {
            string[] splittedMessage = message.Split(' ');
            string messageCode = splittedMessage[0];

            if (messageCode == "REPLY")
            {
                HandleReplyMessage(splittedMessage[1], splittedMessage);
            }
            else if (messageCode == "MSG")
            {
                HandleChatMessage(splittedMessage[2], splittedMessage);
            }
            else if (messageCode == "ERR")
            {
                HandleErrorMessage(splittedMessage);
            }
            else if (messageCode == "BYE\r\n")
            {
                HandleByeMessage();
            }
            else
            {
                throw new Exception($"Unexpected server response '{message}'");
            }
        }

        protected override void HandleReplyMessage(string status, string[] splittedMessage)
        {
            var messageContent = string.Join(" ", splittedMessage.Skip(3));
            
            if (status == "OK")
            {
                Console.Error.Write($"Success: {messageContent}");
            }
            else if (status == "NOK")
            {
                Console.Error.Write($"Failure: {messageContent}");
            }
            else
            {
                throw new Exception($"Unexpected server response code '{status}'");
            }
        }

        protected override void HandleChatMessage(string displayName, string[] splittedMessage)
        {
            var messageContent = string.Join(" ", splittedMessage.Skip(4));
            Console.Write($"{displayName}: {messageContent}");
        }

        protected override void HandleErrorMessage(string[] splittedMessage)
        {
            var messageContent = string.Join(" ", splittedMessage.Skip(4));
            Console.Error.Write($"ERROR: {messageContent}");
        }

        protected override void HandleByeMessage()
        {
            Console.WriteLine("Server has ended the connection. Exiting application");
            CloseConnection();
            Environment.Exit(0);
        }
        
        protected void CloseConnection()
        {
            if (networkStream != null)
            {
                networkStream.Close();
                networkStream = null;  
            }
            tcpClient.Close();  
        }
    }
}