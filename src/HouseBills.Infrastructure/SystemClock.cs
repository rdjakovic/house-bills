using HouseBills.Application.Common;

namespace HouseBills.Infrastructure;

internal sealed class SystemClock(TimeProvider timeProvider) : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
}