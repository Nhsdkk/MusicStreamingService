using NpgsqlTypes;

namespace MusicStreamingService.Data.Entities;

public sealed record SubscriptionEntity : BaseUpdatableIdEntity
{
    /// <summary>
    /// Name of the subscription
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Amount, that need to be payed each time
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Interval between 2 payments
    /// </summary>
    public TimeSpan Period { get; set; }

    /// <summary>
    /// Flag, that checks if subscription is still available for purchase 
    /// </summary>
    public bool Discontinued { get; set; } = false;
}