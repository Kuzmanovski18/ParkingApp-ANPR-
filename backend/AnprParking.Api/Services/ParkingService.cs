using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AnprParking.Api.Domain.Entities;
using AnprParking.Api.Domain.Enums;
using AnprParking.Api.Infrastructure;
using AnprParking.Api.Services.Pricing;

namespace AnprParking.Api.Services;

public class ParkingService
{
    private readonly AppDbContext _db;
    private readonly IPricingService _pricing;
    private readonly IHttpContextAccessor _http;

    public ParkingService(AppDbContext db, IPricingService pricing, IHttpContextAccessor http)
    {
        _db = db;
        _pricing = pricing;
        _http = http;
    }

    public static string NormalizePlate(string p)
        => new string((p ?? "").Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

    private string? CurrentUserId =>
        _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task<bool> IsActiveMemberAsync(string plate, DateTime nowUtc, CancellationToken ct)
    {
        return await _db.MemberVehicles.AnyAsync(m =>
            m.Plate == plate &&
            m.ValidFromUtc <= nowUtc &&
            nowUtc < m.ValidUntilUtc, ct);
    }

    public async Task<(bool IsMember, Guid? SessionId)> HandleEntryAsync(
        string plate,
        string source,
        string raw,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        plate = NormalizePlate(plate);

        await LogAsync("ENTRY", plate, source, raw, ct);

        // Member => no session needed
        if (await IsActiveMemberAsync(plate, now, ct))
            return (true, null);

        // Already has active session
        var existing = await _db.ParkingSessions.FirstOrDefaultAsync(
            s => s.Plate == plate && s.Status != SessionStatus.Closed, ct);

        if (existing != null)
        {
            //  TESTING FRIENDLY: reset timer on each entry (so you don't see 90+ mins)
            existing.EntryUtc = now;
            existing.BillingAnchorUtc = now;
            existing.PaidAtUtc = null;
            existing.GraceUntilUtc = null;
            existing.Status = SessionStatus.Active;

            await _db.SaveChangesAsync(ct);
            return (false, existing.Id);
        }

        var session = new ParkingSession
        {
            Id = Guid.NewGuid(),
            Plate = plate,
            EntryUtc = now,
            BillingAnchorUtc = now,
            Status = SessionStatus.Active
        };

        _db.ParkingSessions.Add(session);
        await _db.SaveChangesAsync(ct);

        return (false, session.Id);
    }

    public async Task<(bool IsMember, decimal Amount, DateTime? GraceUntilUtc, Guid? SessionId)> QuoteAndMarkPaidAsync(
        string plate,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        plate = NormalizePlate(plate);

        // Member => pays 0
        if (await IsActiveMemberAsync(plate, now, ct))
            return (true, 0m, null, null);

        var session = await _db.ParkingSessions
            .OrderByDescending(s => s.EntryUtc)
            .FirstOrDefaultAsync(s => s.Plate == plate && s.Status != SessionStatus.Closed, ct)
            ?? throw new InvalidOperationException("No active session for this plate.");

        //  NO GRACE LOGIC ANYMORE

        var amount = _pricing.CalculateAmount(session.BillingAnchorUtc, now);

        //  Mark as paid (NO grace window)
        session.PaidAtUtc = now;
        session.GraceUntilUtc = null;
        session.Status = SessionStatus.Active;

        //  move billing window forward so next quote starts from now
        session.BillingAnchorUtc = now;

        _db.PaymentRecords.Add(new PaymentRecord
        {
            Id = Guid.NewGuid(),
            Plate = plate,
            Amount = amount,
            Currency = "mkd",
            PaidAtUtc = now,
            ParkingSessionId = session.Id,
            Provider = "Manual",
            ProviderRef = "",
            Kind = "Parking",
            UserId = CurrentUserId
        });

        await _db.SaveChangesAsync(ct);

        return (false, amount, null, session.Id);
    }

    public async Task<string> HandleExitAsync(
        string plate,
        string source,
        string raw,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        plate = NormalizePlate(plate);

        await LogAsync("EXIT", plate, source, raw, ct);

        // Member => always allowed
        if (await IsActiveMemberAsync(plate, now, ct))
            return "EXIT_ALLOWED_MEMBER";

        var session = await _db.ParkingSessions.FirstOrDefaultAsync(
            s => s.Plate == plate && s.Status != SessionStatus.Closed, ct);

        if (session == null)
            return "NO_SESSION";

        //  Allow exit only if paid
        if (session.PaidAtUtc.HasValue)
        {
            // if you have ClosedAtUtc, you can set it here (optional):
            // session.ClosedAtUtc = now;

            session.Status = SessionStatus.Closed;
            await _db.SaveChangesAsync(ct);
            return "EXIT_ALLOWED_PAID_SESSION_CLOSED";
        }

        return "EXIT_DENIED_NOT_PAID";
    }

    private async Task LogAsync(string eventType, string plate, string source, string raw, CancellationToken ct)
    {
        _db.AnprEventLogs.Add(new AnprEventLog
        {
            Id = Guid.NewGuid(),
            CreatedUtc = DateTime.UtcNow,
            Plate = plate,
            EventType = eventType,
            Source = source,
            RawPayload = raw ?? ""
        });

        await _db.SaveChangesAsync(ct);
    }
}
