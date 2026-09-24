namespace HouseBills.Domain;

/// <summary>Who a bill is paid to, e.g. the electricity provider.</summary>
public sealed class Payee : Entity
{
    public const int NameMaxLength = 150;
    public const int AccountReferenceMaxLength = 100;
    public const int NotesMaxLength = 500;

    // Used by EF Core when materializing.
    private Payee()
    {
        Name = string.Empty;
    }

    public Payee(string name, string? accountReference, string? notes)
    {
        Name = Guard.RequiredText(name, NameMaxLength, nameof(name));
        AccountReference = Guard.OptionalText(accountReference, AccountReferenceMaxLength, nameof(accountReference));
        Notes = Guard.OptionalText(notes, NotesMaxLength, nameof(notes));
    }

    public string Name { get; private set; }

    /// <summary>Customer / contract number with the payee.</summary>
    public string? AccountReference { get; private set; }

    public string? Notes { get; private set; }

    public void Update(string name, string? accountReference, string? notes)
    {
        Name = Guard.RequiredText(name, NameMaxLength, nameof(name));
        AccountReference = Guard.OptionalText(accountReference, AccountReferenceMaxLength, nameof(accountReference));
        Notes = Guard.OptionalText(notes, NotesMaxLength, nameof(notes));
    }
}