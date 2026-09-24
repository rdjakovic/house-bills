namespace HouseBills.Application.Common;

/// <summary>An expected business failure. <see cref="Message"/> is user-facing.</summary>
public sealed record Error(ErrorKind Kind, string Message)
{
    public const string ConflictMessage = "This record was changed or deleted by someone else. Reload and try again.";

    public static Error Validation(string message) => new(ErrorKind.Validation, message);

    public static Error NotFound(string message) => new(ErrorKind.NotFound, message);

    public static Error Conflict() => new(ErrorKind.Conflict, ConflictMessage);
}