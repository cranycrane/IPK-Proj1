using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using IPK_Proj1.Clients;
using IPK_Proj1.Messages;

namespace IPK_Proj1_Tests;

[Collection("Network Tests")]
public class ClientTcpHandleTests : IDisposable
{
    private readonly StringWriter outputWriter;
    private readonly StringWriter errorWriter;
    private readonly TextWriter originalOutput;
    private readonly TextWriter originalError;

    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private readonly List<string> _receivedMessages;
    private readonly object _lock = new object();
    private TcpClient? connectedClient;
    private ClientTcp client;

    public IReadOnlyList<string> ReceivedMessages => _receivedMessages;


    public ClientTcpHandleTests()
    {
        originalOutput = Console.Out;
        originalError = Console.Error;

        outputWriter = new StringWriter();
        errorWriter = new StringWriter();

        Console.SetOut(outputWriter);
        Console.SetError(errorWriter);

        _cts = new CancellationTokenSource();
        _receivedMessages = new List<string>();
        int port = 12345;

        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        _listener = new TcpListener(localAddr, port);
        _listener.Start();

        client = new ClientTcp("127.0.0.1", 12345);
        var listen = client.ListenForMessagesAsync(_cts.Token);

        Task.Run(async () =>
        {
            await StartTestServer(port, message =>
            {
                lock (_lock)
                {
                    _receivedMessages.Add(message);
                }
            }, _cts.Token);
        });

    }

    private async Task StartTestServer(int port, Action<string> onMessageReceived, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_listener.Pending())
                {
                    using var client = await _listener.AcceptTcpClientAsync();
                    connectedClient = client;
                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream, Encoding.ASCII);

                    var message = await reader.ReadLineAsync();
                    onMessageReceived(message);
                }
                else
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
        }
        finally
        {
            _listener.Stop();
        }
    }

    private async Task SendMessageToClient(string message)
    {
        if (connectedClient == null || !connectedClient.Connected)
            throw new InvalidOperationException("Client is not connected.");

        NetworkStream stream = connectedClient.GetStream();

        byte[] messageBytes = Encoding.ASCII.GetBytes(message);

        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }


    public void Dispose()
    {
        Console.SetOut(originalOutput);
        Console.SetError(originalError);

        _cts.Cancel();
        client.Disconnect();
        _listener.Stop();
        outputWriter.Dispose();
        errorWriter.Dispose();
    }

    [Fact]
    public async Task HandleReplyOkMessage_Test()
    {
        string testMessage = "REPLY OK IS 123 Example reply message";
        await SendMessageToClient(testMessage);
        await Task.Delay(100);
        Assert.Equal("Success: 123 Example reply message\n", errorWriter.ToString());
    }

    [Fact]
    public async Task HandleReplyNOkMessage_Test()
    {
        string testMessage = "REPLY NOK IS 123 Example ERROR reply message";
        await SendMessageToClient(testMessage);
        await Task.Delay(100);
        Assert.Equal("Failure: 123 Example ERROR reply message\n", errorWriter.ToString());
    }

    [Fact]
    public async Task HandleChatMessage_Test()
    {
        string testMessage = "MSG FROM Serverik IS Ahoj tohle je zprava!!!";
        await SendMessageToClient(testMessage);
        await Task.Delay(100);
        Assert.Equal("Serverik: Ahoj tohle je zprava!!!\n", outputWriter.ToString());
    }

    [Fact]
    public async Task HandleErrorMessage_Test()
    {
        string testMessage = "ERR FROM Serverik IS This is error message";
        await SendMessageToClient(testMessage);
        await Task.Delay(100);
        Assert.Equal("ERR FROM Serverik: This is error message\n", errorWriter.ToString());
    }
}