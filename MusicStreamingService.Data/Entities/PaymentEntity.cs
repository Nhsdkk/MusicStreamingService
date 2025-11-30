namespace MusicStreamingService.Data.Entities;

public sealed record PaymentEntity : BaseIdEntity
{
    /// <summary>
    /// Id of the payer
    /// </summary>
    public Guid PayerId { get; set; }

    /// <summary>
    /// Payer
    /// </summary>
    public UserEntity Payer { get; set; } = null!;
    
    /// <summary>
    /// Id of the corresponding subscription
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Corresponding subscription 
    /// </summary>
    public SubscriptionEntity Subscription { get; set; } = null!;
    
    /// <summary>
    /// Amount paid
    /// </summary>
    public decimal Amount { get; set; }
}