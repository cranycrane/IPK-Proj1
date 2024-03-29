using IPK_Proj1.Commands;

namespace IPK_Proj1_Tests;

public class HelpCommandTests
{
    [Fact]
    public void ValidateArgs_WithInvalidNumberOfParameters_ThrowsArgumentException()
    {
        var command = new HelpCommand();
        string[] parameters = new string[] { "invalidarg",};

        var exception = Assert.Throws<ArgumentException>(() => command.ValidateArgs(parameters));

        Assert.Equal("ERR: Unexpected number of parameters in a command", exception.Message);
    }

}