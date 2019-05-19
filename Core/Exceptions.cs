using System;

namespace MusicMachine
{
public class StateMalformedException : Exception
{
    public StateMalformedException()
    {
    }
    public StateMalformedException(string message)
        : base(message)
    {
    }
    public StateMalformedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
///     This exception is to indicate an exception that should never be thrown. If it is, there is something wrong or there
///     is a
///     bUg
/// </summary>
public class ShouldNotHappenException : Exception
{
    private const string DefaultMessage =
        "This exception should never have been thrown! Go yell at the developers!";

    public ShouldNotHappenException()
        : this(DefaultMessage)
    {
    }
    public ShouldNotHappenException(Exception innerException)
        : base(DefaultMessage, innerException)
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