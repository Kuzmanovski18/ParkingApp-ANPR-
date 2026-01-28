using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using AnprParking.Api.Infrastructure;
using System.Security.Claims;

namespace AnprParking.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/checkout")]
public class CheckoutController : ControllerBase
{
    private readonly AppDbContext _db;

    public CheckoutController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/checkout/confirm?session_id=...
    [HttpGet("confirm")]
    public async Task<ActionResult> Confirm([FromQuery] string session_id)
    {
        if (string.IsNullOrWhiteSpace(session_id))
            return BadRequest("Missing session_id");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var service = new SessionService();
        var session = await service.GetAsync(session_id);

        if (session.PaymentStatus != "paid")
            return BadRequest("Payment not completed.");

        var plate = session.Metadata.GetValueOrDefault("plate");
        var type = session.Metadata.GetValueOrDefault("type"); // Monthly | Yearly

        if (string.IsNullOrWhiteSpace(plate))
            return BadRequest("Missing plate metadata.");

        // 👉 Запиши PaymentRecord
        _db.PaymentRecords.Add(new Domain.Entities.PaymentRecord
        {
            Plate = plate,
            Amount = session.AmountTotal.GetValueOrDefault() / 100m,
            Currency = session.Currency ?? "mkd",
            PaidAtUtc = DateTime.UtcNow,
            Provider = "Stripe",
            ProviderRef = session.Id,
            Kind = "Membership",
            UserId = userId
        });

        await _db.SaveChangesAsync();

        return Ok(new
        {
            plate,
            type,
            amount = session.AmountTotal.GetValueOrDefault() / 100m
        });
    }
}
