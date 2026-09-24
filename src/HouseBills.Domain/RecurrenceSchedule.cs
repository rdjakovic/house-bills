namespace HouseBills.Domain;

/// <summary>
/// When a recurring bill falls due. Occurrence <c>n</c> is computed from <see cref="StartDate"/> (not from the
/// previous occurrence) so month-end dates don't drift: a schedule starting Jan 31 yields Feb 28, Mar 31, Apr 30...
/// </summary>
public readonly record struct RecurrenceSchedule
{
    public RecurrenceSchedule(BillFrequency frequency, DateOnly startDate, DateOnly? endDate)
    {
        if (!Enum.IsDefined(frequency))
        {
            throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Unknown frequency.");
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("End date must not be before the start date.", nameof(endDate));
        }

        Frequency = frequency;
        StartDate = startDate;
        EndDate = endDate;
    }

    public BillFrequency Frequency { get; }

    /// <summary>The first due date.</summary>
    public DateOnly StartDate { get; }

    /// <summary>Last date an occurrence may fall on (inclusive), or <c>null</c> for open-ended.</summary>
    public DateOnly? EndDate { get; }

    /// <summary>All due dates within <paramref name="from"/>..<paramref name="to"/> (both inclusive).</summary>
    public IEnumerable<DateOnly> GetDueDates(DateOnly from, DateOnly to)
    {
        var last = EndDate is { } end && end < to ? end : to;
        for (var n = 0; ; n++)
        {
            var date = Occurrence(n);
            if (date > last)
            {
                yield break;
            }

            if (date >= from)
            {
                yield return date;
            }
        }
    }

    /// <summary>The first due date on or after <paramref name="date"/>, or <c>null</c> if the schedule has ended.</summary>
    public DateOnly? NextDueDateOnOrAfter(DateOnly date)
    {
        for (var n = 0; ; n++)
        {
            var occurrence = Occurrence(n);
            if (EndDate is { } end && occurrence > end)
            {
                return null;
            }

            if (occurrence >= date)
            {
                return occurrence;
            }
        }
    }

    private DateOnly Occurrence(int n) => Frequency switch
    {
        BillFrequency.Weekly => StartDate.AddDays(7 * n),
        BillFrequency.Monthly => StartDate.AddMonths(n),
        BillFrequency.Quarterly => StartDate.AddMonths(3 * n),
        BillFrequency.Yearly => StartDate.AddYears(n),
        _ => throw new InvalidOperationException($"Unknown frequency {Frequency}."),
    };
}