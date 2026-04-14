namespace eVisaPlatform.Application.Interfaces;

/// <summary>
/// Abstracts DateTime.UtcNow to allow deterministic unit testing.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
