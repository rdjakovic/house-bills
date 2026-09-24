using HouseBills.Application.Resources;

namespace HouseBills.Application.Common;

/// <summary>An expected business failure. <see cref="Message"/> is user-facing, in the current UI language.</summary>
public sealed record Error(ErrorKind Kind, string Message)
{
    public static Error Validation(string message) => new(ErrorKind.Validation, message);

    public static Error NotFound(string message) => new(ErrorKind.NotFound, message);

    public static Error Conflict() => new(ErrorKind.Conflict, Messages.Conflict);
}