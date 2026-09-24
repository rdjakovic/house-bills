namespace HouseBills.Application.Categories;

public sealed record CategoryDto(int Id, string Name, byte[] RowVersion);