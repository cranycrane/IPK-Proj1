using System;

namespace IPK_Proj1.Exceptions;

public class UnknownCommandException : System.Exception
{
    public UnknownCommandException(string message)
        : base(message)
    {
    }   
}