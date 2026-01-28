using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using AnprParking.Api.Services;

namespace AnprParking.Api.Controllers;

[ApiController]
[Route("api/stripe")]
public class StripeController : ControllerBase
{
    private readonly ParkingService _parking;

    public StripeController(ParkingService parking)
    {
        _parking = parking;
    }

    [HttpPost("confirm")]
    [Authorize]
    public async Task<IActionResult> Confirm([FromQuery] string sessionId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest("sessionId is required.");

        var service = new SessionService();
        var session = await service.GetAsync(sessionId, cancellationToken: ct);

        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            return BadRequest($"PaymentStatus is '{session.PaymentStatus}', not paid.");

        if (session.Metadata == null || !session.Metadata.TryGetValue("kind", out var kind))
            return BadRequest("Missing metadata.kind");

        if (kind == "Parking")
        {
            if (!session.Metadata.TryGetValue("plate", out var plate) || string.IsNullOrWhiteSpace(plate))
                return BadRequest("Missing metadata.plate");

            var q = await _parking.QuoteAndMarkPaidAsync(plate, ct);
            return Ok(new
            {
                kind = "Parking",
                plate,
                q.IsMember,
                q.Amount,
                q.GraceUntilUtc,
                q.SessionId
            });
        }

        return Ok(new { kind }); // membership може посебно ако сакаш
    }
}
