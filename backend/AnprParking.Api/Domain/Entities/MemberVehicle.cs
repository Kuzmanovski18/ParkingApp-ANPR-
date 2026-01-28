namespace AnprParking.Api.Domain.Entities;

public class MemberVehicle
{
    public Guid Id { get; set; }

    // Plate with active membership (stored normalized)
    public string Plate { get; set; } = default!;

    // Membership validity window
    public DateTime ValidFromUtc { get; set; }
    public DateTime ValidUntilUtc { get; set; }

    // Monthly | Yearly
    public string Type { get; set; } = default!;

    // Optional owner info (from checkout)
    public string OwnerName { get; set; } = "";
    public string OwnerEmail { get; set; } = "";

    //  Link to authenticated user (for MyCars / Profile)
    public string UserId { get; set; } = default!;

    // Stripe references (for audit / debugging)
    public string StripeSessionId { get; set; } = "";

    // Helper
    public bool IsActive => DateTime.UtcNow >= ValidFromUtc && DateTime.UtcNow <= ValidUntilUtc;
}
