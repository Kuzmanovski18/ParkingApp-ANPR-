using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnprParking.Api.Domain.Enums;
using AnprParking.Api.Infrastructure;
using AnprParking.Api.Services.Pricing;

namespace AnprParking.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPricingService _pricing;

    public AdminController(AppDbContext db, IPricingService pricing)
    {
        _db = db;
        _pricing = pricing;
    }

    [HttpGet("active-sessions")]
    public async Task<IActionResult> ActiveSessions(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var sessions = await _db.ParkingSessions
            .Where(s => s.Status != SessionStatus.Closed)
            .OrderByDescending(s => s.EntryUtc)
            .Select(s => new
            {
                s.Id,
                s.Plate,
                s.EntryUtc,
                s.BillingAnchorUtc,
                status = s.Status.ToString(),
                s.PaidAtUtc,
                s.GraceUntilUtc
            })
            .ToListAsync(ct);

        // додај isMember + currentAmount
        var result = new List<object>();
        foreach (var s in sessions)
        {
            var isMember = await _db.MemberVehicles.AnyAsync(m =>
                m.Plate == s.Plate &&
                m.ValidFromUtc <= now &&
                now < m.ValidUntilUtc, ct);

            decimal amount = 0m;
            if (!isMember)
            {
                //  ако имаш grace mode и сакаш да е 0 во grace, можеш тука да условиш.
                // ама ти сакаш да почнува од 30 веднаш -> pricing ќе го врати тоа ако го направиш така.
                amount = _pricing.CalculateAmount(s.BillingAnchorUtc, now);
                if (amount < 30m) amount = 30m; //  старт од 30 за активна сесија
            }

            result.Add(new
            {
                id = s.Id,
                plate = s.Plate,
                entryUtc = s.EntryUtc,
                status = s.status,
                isMember,
                currentAmount = amount,
                graceUntilUtc = (DateTime?)null // ти сакаш да не прикажуваш grace
            });
        }

        return Ok(result);
    }

    // другите endpoints остави ги како што се
}
