using System.Net.Sockets;
using System.Text;
using IPK_Proj1.Messages;
using System.Threading;

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

        public override async Task Send(IMessage message)
        {
            // Pokud není zpráva typu ByeMessage a již čekáme na odpověď, přerušit
            if (IsWaitingReply && message.GetType() != typeof(ByeMessage))
            {
                Logger.Debug("Waiting for reply, can't send");
                return;
            }

            await SendSemaphore.WaitAsync(); // Počká, dokud nebude možné vstoupit

            try
            {
                IsWaitingReply = message.IsAwaitingReply;

                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message.ToTcpString());
                await networkStream!.WriteAsync(data, 0, data.Length);
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync($"An error occurred: {e.Message}");
            }
            finally
            {
                if (!IsWaitingReply)
                {
                    SendSemaphore.Release(); // Uvolní semafor, pokud nečekáme na odpověď
                }
            }
        }

        public string? Receive()
        {
            return reader.ReadLine();
        }

        public override bool Connected()
        {
            return tcpClient.Connected;
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
                        // var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        try
                        {
                            await HandleServerMessage(buffer, bytesRead);
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


        protected override async Task HandleServerMessage(byte[] receivedBytes, int bytesRead)
        {
            var messageString = Encoding.UTF8.GetString(receivedBytes, 0, bytesRead);
            messageString = messageString.Replace("\r\n", "");
            
            string[] splittedMessage = messageString.Split(' ');
            string messageCode = messageString.Split(' ')[0];

            if (messageCode == "REPLY")
            {
                IsWaitingReply = false;
                SendSemaphore.Release();
                await HandleReplyMessage(new ReplyMessage(string.Join(" ", splittedMessage.Skip(3)), splittedMessage[1]));
            }
            else if (messageCode == "MSG")
            {
                HandleChatMessage(new ChatMessage(splittedMessage[2], string.Join(" ", splittedMessage.Skip(4))));
            }
            else if (messageCode == "ERR")
            {
                await HandleErrorMessage(new ErrorMessage(splittedMessage[2], string.Join(" ", splittedMessage.Skip(4))));
            }
            else if (messageCode == "BYE\r\n")
            {
                HandleByeMessage();
            }
            else
            {
                throw new Exception($"Unexpected server response code '{messageCode}'");
            }
        }
        

        protected override void HandleByeMessage()
        {
            Disconnect();
            Environment.Exit(0);
        }
        
        public override void Disconnect()
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