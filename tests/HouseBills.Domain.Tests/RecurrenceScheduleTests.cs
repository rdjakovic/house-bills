using HouseBills.Domain;

namespace HouseBills.Domain.Tests;

public sealed class RecurrenceScheduleTests
{
    [Fact]
    public void GetDueDates_MonthlyFromMonthEnd_ClampsWithoutDrifting()
    {
        var schedule = new RecurrenceSchedule(BillFrequency.Monthly, new DateOnly(2026, 1, 31), null);

        var dates = schedule.GetDueDates(new DateOnly(2026, 1, 1), new DateOnly(2026, 5, 31)).ToList();

        dates.ShouldBe([
            new DateOnly(2026, 1, 31),
            new DateOnly(2026, 2, 28),
            new DateOnly(2026, 3, 31),
            new DateOnly(2026, 4, 30),
            new DateOnly(2026, 5, 31),
        ]);
    }

    [Theory]
    [InlineData(BillFrequency.Weekly, 5)]
    [InlineData(BillFrequency.Monthly, 2)]
    [InlineData(BillFrequency.Quarterly, 1)]
    [InlineData(BillFrequency.Yearly, 1)]
    public void GetDueDates_EachFrequency_ReturnsExpectedCount(BillFrequency frequency, int expected)
    {
        var schedule = new RecurrenceSchedule(frequency, new DateOnly(2026, 3, 1), null);

        var dates = schedule.GetDueDates(new DateOnly(2026, 3, 1), new DateOnly(2026, 4, 1)).ToList();

        dates.Count.ShouldBe(expected);
    }

    [Fact]
    public void GetDueDates_YearlyFromLeapDay_FallsOnFebruary28InCommonYears()
    {
        var schedule = new RecurrenceSchedule(BillFrequency.Yearly, new DateOnly(2024, 2, 29), null);

        var dates = schedule.GetDueDates(new DateOnly(2024, 1, 1), new DateOnly(2028, 12, 31)).ToList();

        dates.ShouldBe([
            new DateOnly(2024, 2, 29),
            new DateOnly(2025, 2, 28),
            new DateOnly(2026, 2, 28),
            new DateOnly(2027, 2, 28),
            new DateOnly(2028, 2, 29),
        ]);
    }

    [Fact]
    public void GetDueDates_WithEndDate_StopsAtEndDate()
    {
        var schedule = new RecurrenceSchedule(BillFrequency.Monthly, new DateOnly(2026, 1, 10), new DateOnly(2026, 3, 10));

        var dates = schedule.GetDueDates(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)).ToList();

        dates.ShouldBe([new DateOnly(2026, 1, 10), new DateOnly(2026, 2, 10), new DateOnly(2026, 3, 10)]);
    }

    [Fact]
    public void GetDueDates_RangeAfterStart_SkipsEarlierOccurrences()
    {
        var schedule = new RecurrenceSchedule(BillFrequency.Monthly, new DateOnly(2026, 1, 15), null);

        var dates = schedule.GetDueDates(new DateOnly(2026, 3, 1), new DateOnly(2026, 4, 30)).ToList();

        dates.ShouldBe([new DateOnly(2026, 3, 15), new DateOnly(2026, 4, 15)]);
    }

    [Fact]
    public void NextDueDateOnOrAfter_BetweenOccurrences_ReturnsNextOne()
    {
        var schedule = new RecurrenceSchedule(BillFrequency.Quarterly, new DateOnly(2026, 1, 5), null);

        schedule.NextDueDateOnOrAfter(new DateOnly(2026, 2, 1)).ShouldBe(new DateOnly(2026, 4, 5));
    }

    [Fact]
    public void NextDueDateOnOrAfter_AfterEndDate_ReturnsNull()
    {
        var schedule = new RecurrenceSchedule(BillFrequency.Monthly, new DateOnly(2026, 1, 5), new DateOnly(2026, 2, 5));

        schedule.NextDueDateOnOrAfter(new DateOnly(2026, 2, 6)).ShouldBeNull();
    }

    [Fact]
    public void Constructor_EndBeforeStart_Throws()
    {
        Should.Throw<ArgumentException>(() => new RecurrenceSchedule(BillFrequency.Monthly, new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)));
    }
}