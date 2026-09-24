namespace HouseBills.Application.Common;

/// <summary>
/// Thrown by repositories when a row was changed or deleted since it was loaded (optimistic concurrency).
/// Services translate it to <see cref="ErrorKind.Conflict"/>.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
    {
    }

    public ConcurrencyConflictException(string message)
        : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}