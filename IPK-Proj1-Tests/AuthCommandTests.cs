using IPK_Proj1.Commands;

namespace IPK_Proj1_Tests;

public class AuthCommandTests
{
    [Fact]
    public void ValidateArgs_WithInvalidNumberOfParameters_ThrowsArgumentException()
    {
        var command = new AuthCommand();
        string[] parameters = new string[] {}; // Neplatný počet parametrů

        var exception = Assert.Throws<ArgumentException>(() => command.ValidateArgs(parameters));

        Assert.Equal("ERR: Unexpected number of parameters in a command", exception.Message);
    }

    [Fact]
    public void ValidateArgs_WithInvalidUsername_ThrowsArgumentException()
    {
        var command = new AuthCommand();
        string[] parameters = new string[] { "invalid/*>sername", "validSecret123", "ValidDisplayName" };

        var exception = Assert.Throws<ArgumentException>(() => command.ValidateArgs(parameters));

        Assert.Equal("ERR: Username must contain only A-Z, a-z, 0-9 and maximum of 20 characters", exception.Message);
    }
    
    [Fact]
    public void ValidateArgs_WithInvalidSecret_ThrowsArgumentException()
    {
        var command = new AuthCommand();
        string[] parameters = new string[] { "validusername", "invalid/*secr♥etjupikdupik", "ValidDisplayName" };

        var exception = Assert.Throws<ArgumentException>(() => command.ValidateArgs(parameters));

        Assert.Equal("ERR: Secret must contain only printable characters and maximum of 128", exception.Message);
    }

}