using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using AnprParking.Api.Infrastructure;
using AnprParking.Api.Services;
using AnprParking.Api.Services.Pricing;
using AnprParking.Api.Domain.Enums;

namespace AnprParking.Api.Controllers;

public record ParkingCheckoutRequest(string Plate);
public record ParkingCheckoutResponse(string Url, string SessionId, decimal Amount);

[ApiController]
[Route("api/parking")]
public class ParkingController : ControllerBase
{
    private readonly IConfiguration _cfg;
    private readonly AppDbContext _db;
    private readonly IPricingService _pricing;

    public ParkingController(IConfiguration cfg, AppDbContext db, IPricingService pricing)
    {
        _cfg = cfg;
        _db = db;
        _pricing = pricing;
    }

    // Parking payment via Stripe (Checkout) -  DYNAMIC amount
    [HttpPost("checkout")]
    [Authorize]
    public async Task<ActionResult<ParkingCheckoutResponse>> CreateParkingCheckout(
        [FromBody] ParkingCheckoutRequest req,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Plate))
            return BadRequest("Plate is required.");

        var plate = ParkingService.NormalizePlate(req.Plate);
        var now = DateTime.UtcNow;

        var success = _cfg["Stripe:SuccessUrl"];
        var cancel = _cfg["Stripe:CancelUrl"];

        if (string.IsNullOrWhiteSpace(success) || string.IsNullOrWhiteSpace(cancel))
            return BadRequest("Missing Stripe SuccessUrl/CancelUrl in config.");

        // 1) Ако е активен member -> 0 ден
        var isMember = await _db.MemberVehicles.AnyAsync(m =>
            m.Plate == plate &&
            m.ValidFromUtc <= now &&
            now < m.ValidUntilUtc, ct);

        if (isMember)
            return BadRequest("This plate has an active membership. Amount is 0.");

        // 2) Најди активна parking сесија
        var session = await _db.ParkingSessions
            .OrderByDescending(s => s.EntryUtc)
            .FirstOrDefaultAsync(s => s.Plate == plate && s.Status != SessionStatus.Closed, ct);

        if (session is null)
            return BadRequest("No active session for this plate.");

        // 3) Ако grace истекол, rollover (како во ParkingService)
        if (session.Status == SessionStatus.PaymentGrace &&
            session.GraceUntilUtc.HasValue &&
            now > session.GraceUntilUtc.Value)
        {
            session.BillingAnchorUtc = session.GraceUntilUtc.Value;
            session.PaidAtUtc = null;
            session.GraceUntilUtc = null;
            session.Status = SessionStatus.Active;

            await _db.SaveChangesAsync(ct);
        }

        // 4) Пресметај реална цена (30 ден по започнат час)
        var amount = _pricing.CalculateAmount(session.BillingAnchorUtc, now);

        //  ако сакаш да биде минимум 30 веднаш:
        if (amount < 30m) amount = 30m;

        // Stripe: MKD minor units (денари имаат 2 decimals) -> *100
        var unitAmount = (long)(amount * 100m);

        // 5) Stripe checkout
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = success + "?success=1&session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancel,
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "mkd",
                        UnitAmount = unitAmount,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "ANPR Parking Payment",
                            Description = $"Plate: {plate}"
                        }
                    }
                }
            },
            Metadata = new Dictionary<string, string>
            {
                ["kind"] = "Parking",
                ["plate"] = plate,
                ["sessionId"] = session.Id.ToString(),
                ["amount"] = amount.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }
        };

        var service = new SessionService();
        var stripeSession = await service.CreateAsync(options, cancellationToken: ct);

        return Ok(new ParkingCheckoutResponse(stripeSession.Url, stripeSession.Id, amount));
    }

    //  Optional: endpoint за HomeTimerCard (за сите корисници)
    // GET /api/parking/active-session?plate=SK1234AB
    [HttpGet("active-session")]
    public async Task<IActionResult> ActiveSession([FromQuery] string plate, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(plate))
            return BadRequest("Plate is required.");

        var p = ParkingService.NormalizePlate(plate);
        var now = DateTime.UtcNow;

        var isMember = await _db.MemberVehicles.AnyAsync(m =>
            m.Plate == p &&
            m.ValidFromUtc <= now &&
            now < m.ValidUntilUtc, ct);

        if (isMember)
        {
            // Member нема сесија (по твоја логика), ама за UI враќаме status
            return Ok(new
            {
                plate = p,
                entryUtc = now.ToString("O"),
                status = "Member",
                isMember = true,
                currentAmount = 0m,
                graceUntilUtc = (DateTime?)null
            });
        }

        var session = await _db.ParkingSessions
            .OrderByDescending(s => s.EntryUtc)
            .FirstOrDefaultAsync(s => s.Plate == p && s.Status != SessionStatus.Closed, ct);

        if (session is null)
            return NotFound("No active session for this plate.");

        var amount = _pricing.CalculateAmount(session.BillingAnchorUtc, now);
        if (amount < 30m) amount = 30m;

        return Ok(new
        {
            plate = session.Plate,
            entryUtc = session.EntryUtc,
            status = session.Status.ToString(),
            isMember = false,
            currentAmount = amount,
            graceUntilUtc = session.GraceUntilUtc
        });
    }
}
