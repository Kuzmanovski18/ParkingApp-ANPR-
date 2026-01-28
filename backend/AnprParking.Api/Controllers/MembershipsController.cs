using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using AnprParking.Api.Services;

namespace AnprParking.Api.Controllers;

public record CreateCheckoutRequest(string Plate, string Type, string OwnerName, string OwnerEmail);
public record CreateCheckoutResponse(string Url, string SessionId);

[ApiController]
[Route("api/memberships")]
public class MembershipsController : ControllerBase
{
    private readonly IConfiguration _cfg;
    public MembershipsController(IConfiguration cfg) => _cfg = cfg;

    [HttpPost("checkout")]
    public ActionResult<CreateCheckoutResponse> CreateCheckout([FromBody] CreateCheckoutRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Plate))
            return BadRequest("Plate is required.");

        if (string.IsNullOrWhiteSpace(req.OwnerEmail))
            return BadRequest("OwnerEmail is required.");

        var plate = ParkingService.NormalizePlate(req.Plate);

        var type = req.Type?.Trim();
        type = string.Equals(type, "Yearly", StringComparison.OrdinalIgnoreCase) ? "Yearly" : "Monthly";

        var success = _cfg["Stripe:SuccessUrl"];
        var cancel = _cfg["Stripe:CancelUrl"];

        if (string.IsNullOrWhiteSpace(success) || string.IsNullOrWhiteSpace(cancel))
            return BadRequest("Missing Stripe SuccessUrl/CancelUrl in config.");

        //  Amounts in "minor units" (MKD has 2 decimals) -> 1500.00 MKD = 150000
        var amountMkd = type == "Yearly" ? 12000m : 1500m;
        var unitAmount = (long)(amountMkd * 100m);

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = success + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancel,
            CustomerEmail = req.OwnerEmail,
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
                            Name = type == "Yearly" ? "ANPR Membership - Yearly" : "ANPR Membership - Monthly",
                            Description = $"Plate: {plate}"
                        }
                    }
                }
            },
            Metadata = new Dictionary<string, string>
            {
                ["kind"] = "Membership",
                ["plate"] = plate,
                ["type"] = type,
                ["ownerName"] = req.OwnerName ?? "",
                ["ownerEmail"] = req.OwnerEmail
            }
        };

        var service = new SessionService();
        var session = service.Create(options);

        return Ok(new CreateCheckoutResponse(session.Url, session.Id));
    }
}
