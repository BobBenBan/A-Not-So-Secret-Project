using System;

namespace MusicMachine
{
public class ShouldNotHappenException : Exception
{
    public const string DefaultMessage =
        "Something that was asserted to not supposed to happen has happened! Go yell at the developers now!";

    public ShouldNotHappenException()
        : base(DefaultMessage)
    {
    }

    public ShouldNotHappenException(string message)
        : base(message)
    {
    }

    public ShouldNotHappenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
}