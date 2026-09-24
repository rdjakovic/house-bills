using System.Reflection;

using HouseBills.Domain;

namespace HouseBills.Application.Tests;

internal static class TestData
{
    public static readonly DateOnly Today = new(2026, 9, 24);
    public static readonly byte[] RowVersion = [0, 0, 0, 0, 0, 0, 0, 1];

    /// <summary>Simulates an entity loaded from the database (Id and RowVersion are set by persistence).</summary>
    public static T Persisted<T>(T entity, int id)
        where T : Entity
    {
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id);
        typeof(Entity).GetProperty(nameof(Entity.RowVersion))!.SetValue(entity, RowVersion);
        return entity;
    }
}