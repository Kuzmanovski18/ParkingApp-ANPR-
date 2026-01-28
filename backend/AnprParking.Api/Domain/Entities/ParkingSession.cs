using AnprParking.Api.Domain.Enums;

namespace AnprParking.Api.Domain.Entities;

public class ParkingSession
{
    public Guid Id { get; set; }

    // plate should be stored normalized (uppercase, no spaces)
    public string Plate { get; set; } = default!;

    // when car entered
    public DateTime EntryUtc { get; set; }

    // anchor for hourly billing (moves forward after each successful payment if you want)
    public DateTime BillingAnchorUtc { get; set; }

    // last time this session was marked paid (for non-members)
    public DateTime? PaidAtUtc { get; set; }

    // grace window after paying (10 minutes) — if car exits before this, session closes
    public DateTime? GraceUntilUtc { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Active;

    // when session is closed (exit/delete)
    public DateTime? ClosedAtUtc { get; set; }

    // ? link to authenticated user (set when user pays / manages via app)
    public string? UserId { get; set; }

    // ? snapshot: was member at time of pay/quote
    public bool IsMember { get; set; }

    // optional: last computed/paid amount for quick admin overview
    public decimal? LastAmount { get; set; }
}
