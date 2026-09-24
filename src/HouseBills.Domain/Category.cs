namespace HouseBills.Domain;

/// <summary>A grouping for bills, e.g. "Utilities" or "Insurance".</summary>
public sealed class Category : Entity
{
    public const int NameMaxLength = 100;

    // Used by EF Core when materializing.
    private Category()
    {
        Name = string.Empty;
    }

    public Category(string name)
    {
        Name = Guard.RequiredText(name, NameMaxLength, nameof(name));
    }

    public string Name { get; private set; }

    public void Rename(string name)
    {
        Name = Guard.RequiredText(name, NameMaxLength, nameof(name));
    }
}