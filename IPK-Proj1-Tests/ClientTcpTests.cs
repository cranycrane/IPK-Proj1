using System.Net;
using System.Net.Sockets;
using System.Text;
using IPK_Proj1.Clients;
using IPK_Proj1.Messages;

namespace IPK_Proj1_Tests;
[Collection("Network Tests")]
public class ClientTcpTests : IDisposable
{
    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private readonly List<string> _receivedMessages;
    private readonly object _lock = new object();

    public IReadOnlyList<string> ReceivedMessages => _receivedMessages;

    public ClientTcpTests()
    {
        _cts = new CancellationTokenSource();
        _receivedMessages = new List<string>();
        int port = 12345;
        
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        _listener = new TcpListener(localAddr, port);
        _listener.Start();

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

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
    }


    [Fact]
    public async Task SendAuthMessage()
    {
        var client = new ClientTcp("127.0.0.1", 12345);
        var messageToSend = new AuthMessage("username", "secret", "displayName");
        messageToSend.IsAwaitingReply = false;
        await client.Send(messageToSend);
        await Task.Delay(200);
        string? lastReceivedMessage = ReceivedMessages.LastOrDefault();
        Assert.NotNull(lastReceivedMessage);
        Assert.Equal(messageToSend.ToTcpString(), lastReceivedMessage + "\r\n");
        client.Disconnect();
    }

    [Fact]
    public async Task SendByeMessage()
    {
        var client = new ClientTcp("127.0.0.1", 12345);
        var messageToSend = new ByeMessage();
        messageToSend.IsAwaitingReply = false;
        client.IsAuthenticated = true;
        await client.Send(messageToSend);
        await Task.Delay(200);
        string? lastReceivedMessage = ReceivedMessages.LastOrDefault();
        Assert.NotNull(lastReceivedMessage);
        Assert.Equal(messageToSend.ToTcpString(), lastReceivedMessage + "\r\n");
        client.Disconnect();
    }

    [Fact]
    public async Task SendJoinMessage()
    {
        var client = new ClientTcp("127.0.0.1", 12345);
        var messageToSend = new JoinMessage("random-channel", "PeknejPtak");
        messageToSend.IsAwaitingReply = false;
        client.IsAuthenticated = true;
        await client.Send(messageToSend);
        await Task.Delay(200);
        string? lastReceivedMessage = ReceivedMessages.LastOrDefault();
        Assert.NotNull(lastReceivedMessage);
        Assert.Equal(messageToSend.ToTcpString(), lastReceivedMessage + "\r\n");
        client.Disconnect();
    }

    [Fact]
    public async Task SendErrorMessage()
    {
        var client = new ClientTcp("127.0.0.1", 12345);
        var messageToSend = new ErrorMessage("PeknejPtak", "Test ERROR");
        messageToSend.IsAwaitingReply = false;
        client.IsAuthenticated = true;
        await client.Send(messageToSend);
        await Task.Delay(200);
        string? lastReceivedMessage = ReceivedMessages.LastOrDefault();
        Assert.NotNull(lastReceivedMessage);
        Assert.Equal(messageToSend.ToTcpString(), lastReceivedMessage + "\r\n");
        client.Disconnect();
    }
}