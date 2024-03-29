using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using IPK_Proj1.Clients;
using IPK_Proj1.Messages;

namespace IPK_Proj1_Tests;

[Collection("Network Tests")]
public class ClientUdpHandleTests : ClientUdp,IDisposable
{
    private readonly StringWriter outputWriter;
    private readonly StringWriter errorWriter;
    private readonly TextWriter originalOutput;
    private readonly TextWriter originalError;
    
    public ClientUdpHandleTests() : base("127.0.0.1", 12345, 250, 3)
    {
        originalOutput = Console.Out;
        originalError = Console.Error;

        outputWriter = new StringWriter();
        errorWriter = new StringWriter();

        Console.SetOut(outputWriter);
        Console.SetError(errorWriter);
    }
    
    public void Dispose()
    {
        Console.SetOut(originalOutput);
        Console.SetError(originalError);

        outputWriter.Dispose();
        errorWriter.Dispose();
    }
    
    [Fact]
    public async Task HandleReplyOkMessage_Test()
    {
        var message = new ReplyMessage("This is reply message.", "OK", 1, 2);
        byte[] messageBytes = message.ToUdpBytes(1);
        
        await HandleServerMessage(messageBytes, messageBytes.Length);

        Assert.Equal("Success: This is reply message.\n", errorWriter.ToString());
    } 
    
    [Fact]
    public async Task HandleReplyNOkMessage_Test()
    {
        var message = new ReplyMessage("This is ERROR reply message.", "NOK", 1, 2);
        byte[] messageBytes = message.ToUdpBytes(1);
        
        await HandleServerMessage(messageBytes, messageBytes.Length);
        
        Assert.Equal("Failure: This is ERROR reply message.\n", errorWriter.ToString());
    } 
    
    [Fact]
    public async Task HandleChatMessage_Test()
    {
        var message = new ChatMessage("Server", "You are connected", 1);
        byte[] messageBytes = message.ToUdpBytes(1);
        
        await HandleServerMessage(messageBytes, messageBytes.Length);

        Assert.Equal("Server: You are connected\n", outputWriter.ToString());
    } 
    
    [Fact]
    public async Task HandleErrorMessage_Test()
    {
        var message = new ErrorMessage("Serverek", "This is error message", 1);
        byte[] messageBytes = message.ToUdpBytes(1);
        
        await HandleServerMessage(messageBytes, messageBytes.Length);

        Assert.Equal("ERR FROM Serverek: This is error message\n", errorWriter.ToString());
    } 
    
}