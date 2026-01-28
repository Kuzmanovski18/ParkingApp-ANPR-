using System.ComponentModel.DataAnnotations;

namespace AnprParking.Api.Domain.Entities;

public class PaymentRecord
{
    public Guid Id { get; set; }

    // Stored normalized (uppercase)
    [Required]
    [MaxLength(32)]
    public string Plate { get; set; } = default!;

    // Amount in MKD (denars)
    public decimal Amount { get; set; }

    [MaxLength(8)]
    public string Currency { get; set; } = "mkd";

    public DateTime PaidAtUtc { get; set; }

    // Optional link to the parking session that was paid
    public Guid? ParkingSessionId { get; set; }

    // Stripe (or other provider)
    [MaxLength(32)]
    public string Provider { get; set; } = "Stripe";

    // IMPORTANT: unique provider reference (Stripe session id or payment_intent)
    [MaxLength(128)]
    public string ProviderRef { get; set; } = "";

    // Parking | Membership
    [MaxLength(32)]
    public string Kind { get; set; } = "Parking";

    // link to authenticated user (so /api/payments/my works)
    [Required]
    public string UserId { get; set; } = default!;
}
