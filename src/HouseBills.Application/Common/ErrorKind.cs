namespace HouseBills.Application.Common;

public enum ErrorKind
{
    /// <summary>The request is invalid; the message is safe to show to the user.</summary>
    Validation = 1,

    /// <summary>The target record does not exist (it may have been deleted by another user).</summary>
    NotFound = 2,

    /// <summary>The record was changed or deleted by another user since it was loaded.</summary>
    Conflict = 3,
}