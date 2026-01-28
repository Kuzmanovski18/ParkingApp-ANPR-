using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AnprParking.Api.Infrastructure;
using Stripe.Checkout;
using AnprParking.Api.Domain.Entities;
using AnprParking.Api.Services;

namespace AnprParking.Api.Controllers;

public record ConfirmPaymentRequest(string SessionId);

[Authorize]
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/payments/my
    [HttpGet("my")]
    public async Task<ActionResult> MyPayments()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var payments = await _db.PaymentRecords
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PaidAtUtc)
            .Select(p => new
            {
                id = p.Id,
                plate = p.Plate,
                amount = p.Amount,
                currency = p.Currency,
                paidAtUtc = p.PaidAtUtc,
                kind = p.Kind,
                provider = p.Provider
            })
            .ToListAsync();

        return Ok(payments);
    }

    // POST /api/payments/confirm
    [HttpPost("confirm")]
    public async Task<ActionResult> Confirm([FromBody] ConfirmPaymentRequest req)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(req.SessionId))
            return BadRequest("Missing sessionId.");

        // Avoid duplicates if user refreshes success page
        var exists = await _db.PaymentRecords.AnyAsync(p => p.Provider == "Stripe" && p.ProviderRef == req.SessionId);
        if (exists) return Ok(new { ok = true, duplicate = true });

        var sessionService = new SessionService();
        var session = await sessionService.GetAsync(req.SessionId);

        if (session == null)
            return BadRequest("Stripe session not found.");

        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Payment not completed.");

        // Read from metadata that we set in MembershipsController
        var rawPlate = session.Metadata?.GetValueOrDefault("plate") ?? "";
        var plate = ParkingService.NormalizePlate(rawPlate);

        var kind = session.Metadata?.GetValueOrDefault("kind") ?? "Membership"; // Membership | Parking
        var type = session.Metadata?.GetValueOrDefault("type") ?? "";           // Monthly | Yearly

        if (string.IsNullOrWhiteSpace(plate))
            return BadRequest("Missing plate metadata in Stripe session.");

        var amount = (session.AmountTotal ?? 0) / 100m;
        var currency = (session.Currency ?? "mkd").ToLowerInvariant();

        // Save payment record
        _db.PaymentRecords.Add(new PaymentRecord
        {
            Id = Guid.NewGuid(),
            Plate = plate,
            Amount = amount,
            Currency = currency,
            PaidAtUtc = DateTime.UtcNow,
            Provider = "Stripe",
            ProviderRef = session.Id,
            Kind = kind,
            UserId = userId
        });

        // If membership -> upsert MemberVehicle
        if (string.Equals(kind, "Membership", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.UtcNow;

            var effectiveType = string.Equals(type, "Yearly", StringComparison.OrdinalIgnoreCase)
                ? "Yearly"
                : "Monthly";

            var validFrom = now;
            var validUntil = effectiveType == "Yearly"
                ? now.AddYears(1)
                : now.AddMonths(1);

            var existing = await _db.MemberVehicles.FirstOrDefaultAsync(m => m.Plate == plate);

            if (existing == null)
            {
                _db.MemberVehicles.Add(new MemberVehicle
                {
                    Id = Guid.NewGuid(),
                    Plate = plate,
                    ValidFromUtc = validFrom,
                    ValidUntilUtc = validUntil,
                    Type = effectiveType,
                    OwnerEmail = session.CustomerDetails?.Email ?? "",
                    OwnerName = session.CustomerDetails?.Name ?? "",
                    UserId = userId,
                    StripeSessionId = session.Id
                });
            }
            else
            {
                existing.ValidFromUtc = validFrom;
                existing.ValidUntilUtc = validUntil;
                existing.Type = effectiveType;

                // owner info (keep old if stripe missing)
                if (!string.IsNullOrWhiteSpace(session.CustomerDetails?.Email))
                    existing.OwnerEmail = session.CustomerDetails!.Email!;
                if (!string.IsNullOrWhiteSpace(session.CustomerDetails?.Name))
                    existing.OwnerName = session.CustomerDetails!.Name!;

                // IMPORTANT: your UserId is non-nullable string, so check empty
                if (string.IsNullOrWhiteSpace(existing.UserId))
                    existing.UserId = userId;

                existing.StripeSessionId = session.Id;
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            ok = true,
            plate,
            kind,
            amount,
            currency
        });
    }
}
