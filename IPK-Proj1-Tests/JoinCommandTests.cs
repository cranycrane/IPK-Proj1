using IPK_Proj1.Commands;

namespace IPK_Proj1_Tests;

public class JoinCommandTests
{
    [Fact]
    public void ValidateArgs_WithInvalidNumberOfParameters_ThrowsArgumentException()
    {
        var command = new JoinCommand();
        string[] parameters = new string[] {"randomarg", "dalsiarg"};

        var exception = Assert.Throws<ArgumentException>(() => command.ValidateArgs(parameters));

        Assert.Equal("ERR: Unexpected number of parameters in a command", exception.Message);
    }
    
    
    [Fact]
    public void ValidateArgs_WithInvalidChannelName_ThrowsArgumentException()
    {
        var command = new JoinCommand();
        string[] parameters = new string[] { "invalid#channel"};

        var exception = Assert.Throws<ArgumentException>(() => command.ValidateArgs(parameters));

        Assert.Equal("ERR: Channel must contain only A-Z, a-z, 0-9 and maximum of 20 characters", exception.Message);
    }

}