namespace DropBear.Codex.Serialization.Exceptions;

/// <summary>
///     Exception thrown when an error occurs during deserialization.
/// </summary>
public class DeserializationException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeserializationException" /> class.
    /// </summary>
    public DeserializationException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeserializationException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DeserializationException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeserializationException" /> class with a specified error message and
    ///     a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public DeserializationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
