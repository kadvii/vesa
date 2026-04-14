using eVisaPlatform.Application.Interfaces;

namespace eVisaPlatform.Infrastructure.Services;

/// <summary>
/// Production implementation of IDateTimeProvider — simply wraps DateTime.UtcNow.
/// Can be replaced with a fake/mock in unit tests.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
