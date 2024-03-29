using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using IPK_Proj1.Clients;
using IPK_Proj1.Messages;

namespace IPK_Proj1_Tests;

[Collection("Network Tests")]
public class ClientUdpSendTests : IDisposable
{
    private UdpServer udpServer;
    private CancellationTokenSource cts;
    private ConcurrentBag<byte[]> receivedMessages;

    public ClientUdpSendTests()
    {
        cts = new CancellationTokenSource();
        receivedMessages = new ConcurrentBag<byte[]>();

        udpServer = new UdpServer(12345); 
        Task.Run(async () => 
        {
            await foreach (var message in udpServer.StartListeningAsync(cts.Token))
            {
                receivedMessages.Add(message); 
            }
        });
    }

    public void Dispose()
    {
        cts.Cancel(); 
        udpServer.Stop();
        cts.Dispose();
    }
    
    
    [Fact]
    public async Task SendAuthMessage()
    {
        var client = new ClientUdp("127.0.0.1", 12345, 250, 3);
        var messageToSend = new AuthMessage("username", "secret", "displayName");
        messageToSend.IsAwaitingReply = false;
        await client.Send(messageToSend);
        await Task.Delay(200); 

        var lastReceivedBytes = receivedMessages.LastOrDefault(); 
        Assert.NotNull(lastReceivedBytes);

        string lastReceivedMessage = Encoding.ASCII.GetString(lastReceivedBytes.Skip(3).ToArray());

        Assert.Equal(messageToSend.ToUdpBytes(0), lastReceivedBytes);
    }

    [Fact]
    public async Task SendByeMessage()
    {
        var client = new ClientUdp("127.0.0.1", 12345, 250, 3);

        var messageToSend = new ByeMessage();
        messageToSend.IsAwaitingReply = false;
        client.IsAuthenticated = true;
        await client.Send(messageToSend);
        await Task.Delay(200); 

        var lastReceivedBytes = receivedMessages.LastOrDefault(); 
        Assert.NotNull(lastReceivedBytes);

        string lastReceivedMessage = Encoding.ASCII.GetString(lastReceivedBytes.Skip(3).ToArray());

        Assert.Equal(messageToSend.ToUdpBytes(0), lastReceivedBytes);
    }

    [Fact]
    public async Task SendJoinMessage()
    {
        var client = new ClientUdp("127.0.0.1", 12345, 250, 3);

        var messageToSend = new JoinMessage("random-channel", "PeknejPtak");
        messageToSend.IsAwaitingReply = false;
        client.IsAuthenticated = true;
        await client.Send(messageToSend);
        await Task.Delay(200); 

        var lastReceivedBytes = receivedMessages.LastOrDefault(); 
        Assert.NotNull(lastReceivedBytes);

        string lastReceivedMessage = Encoding.ASCII.GetString(lastReceivedBytes.Skip(3).ToArray());

        Assert.Equal(messageToSend.ToUdpBytes(0), lastReceivedBytes);
    }

    [Fact]
    public async Task SendErrorMessage()
    {
        var client = new ClientUdp("127.0.0.1", 12345, 250, 3);
        var messageToSend = new ErrorMessage("PeknejPtak", "Test ERROR");
        messageToSend.IsAwaitingReply = false;
        client.IsAuthenticated = true;
        await client.Send(messageToSend);
        await Task.Delay(200); 

        var lastReceivedBytes = receivedMessages.LastOrDefault(); 
        Assert.NotNull(lastReceivedBytes);

        string lastReceivedMessage = Encoding.ASCII.GetString(lastReceivedBytes.Skip(3).ToArray());

        Assert.Equal(messageToSend.ToUdpBytes(0), lastReceivedBytes);
    } 
}