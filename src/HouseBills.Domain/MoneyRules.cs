namespace HouseBills.Domain;

/// <summary>
/// Rules for monetary amounts. Amounts are stored as <c>decimal(18,2)</c> in a single (local) currency.
/// </summary>
public static class MoneyRules
{
    public const int Precision = 18;
    public const int Scale = 2;
    public const decimal MaxAmount = 9_999_999_999_999_999.99m;
    public const string InvalidAmountMessage = "Amount must be greater than zero, at most 9,999,999,999,999,999.99, with at most two decimal places.";

    public static bool IsValidAmount(decimal value)
    {
        return value > 0m && value <= MaxAmount && decimal.Round(value, Scale) == value;
    }
}