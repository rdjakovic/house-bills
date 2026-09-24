namespace HouseBills.Application.Categories;

/// <summary>Create (<paramref name="Id"/> is <c>null</c>) or update a category.</summary>
public sealed record SaveCategoryRequest(int? Id, string Name, byte[]? RowVersion);