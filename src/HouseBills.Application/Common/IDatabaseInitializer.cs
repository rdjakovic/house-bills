namespace HouseBills.Application.Common;

/// <summary>Prepares the data store before the application first uses it.</summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Makes the database ready for use. For a private per-user database this may create or upgrade it;
    /// shared databases are left untouched (they are deployed with migration scripts).
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken);
}