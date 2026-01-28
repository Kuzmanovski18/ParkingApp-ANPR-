using Microsoft.AspNetCore.Mvc;
using AnprParking.Api.Services;
using AnprParking.Api.Services.PlateRecognition;

namespace AnprParking.Api.Controllers;

public record EntryResult(string Plate, bool IsMember, Guid? SessionId);
public record ExitResult(string Plate, string Result);
public record AnprCallback(string Plate, string Direction, object? Payload);

// DTO за multipart/form-data (Swagger-friendly)
public class AnprImageRequest
{
    // frontend мора да праќа FormData key: "image"
    [FromForm(Name = "image")]
    public IFormFile Image { get; set; } = default!;

    // optional: за дебаг/извор (cameraId, kioskId итн.)
    [FromForm(Name = "sourceRef")]
    public string? SourceRef { get; set; }
}

// OPTIONAL: ако сакаш да поддржиш base64 body (RapidAPI Base64 String стил)
public class AnprBase64Request
{
    public string ImageBase64 { get; set; } = "";
    public string? SourceRef { get; set; }
}

[ApiController]
[Route("api/anpr")]
public class AnprController : ControllerBase
{
    private readonly ParkingService _parking;
    private readonly IPlateRecognizer _recognizer;

    public AnprController(ParkingService parking, IPlateRecognizer recognizer)
    {
        _parking = parking;
        _recognizer = recognizer;
    }

    // ---------------- ENTRY (multipart/form-data) ----------------
    [HttpPost("entry")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<EntryResult>> Entry([FromForm] AnprImageRequest req, CancellationToken ct)
    {
        var image = req.Image;
        if (image is null || image.Length == 0)
            return BadRequest("No image.");

        string rawPlate;
        try
        {
            rawPlate = await _recognizer.RecognizePlateAsync(image, ct);
        }
        catch (Exception ex)
        {
            // ако RapidAPI/Recognizer падне, врати јасно
            return StatusCode(502, new
            {
                error = "Plate recognition failed",
                details = ex.Message
            });
        }

        var plate = ParkingService.NormalizePlate(rawPlate);

        // ако не успее recognition, врати 422 (unprocessable entity)
        if (string.IsNullOrWhiteSpace(plate))
        {
            return UnprocessableEntity(new
            {
                error = "Could not detect a license plate from the image.",
                raw = rawPlate
            });
        }

        var (isMember, sessionId) = await _parking.HandleEntryAsync(
            plate,
            "UPLOAD",
            raw: $"file:{image.FileName}",
            ct: ct
        );

        return Ok(new EntryResult(plate, isMember, sessionId));
    }

    // ---------------- EXIT (multipart/form-data) ----------------
    [HttpPost("exit")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ExitResult>> Exit([FromForm] AnprImageRequest req, CancellationToken ct)
    {
        var image = req.Image;
        if (image is null || image.Length == 0)
            return BadRequest("No image.");

        string rawPlate;
        try
        {
            rawPlate = await _recognizer.RecognizePlateAsync(image, ct);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new
            {
                error = "Plate recognition failed",
                details = ex.Message
            });
        }

        var plate = ParkingService.NormalizePlate(rawPlate);

        if (string.IsNullOrWhiteSpace(plate))
        {
            return UnprocessableEntity(new
            {
                error = "Could not detect a license plate from the image.",
                raw = rawPlate
            });
        }

        var result = await _parking.HandleExitAsync(
            plate,
            "UPLOAD",
            raw: $"file:{image.FileName}",
            ct: ct
        );

        return Ok(new ExitResult(plate, result));
    }

    // ---------------- OPTIONAL: ENTRY via base64 JSON ----------------
    // Ако сакаш од frontend да праќаш { imageBase64: "...", sourceRef: "..." }
    [HttpPost("entry-base64")]
    public async Task<ActionResult<EntryResult>> EntryBase64([FromBody] AnprBase64Request req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ImageBase64))
            return BadRequest("Missing imageBase64.");

        // Тука ти треба Recognizer што знае да работи со base64.
        // Ако твој IPlateRecognizer прима само IFormFile, ова ќе го оставиме како TODO.
        return StatusCode(501, new { error = "Not implemented. Add base64 support to IPlateRecognizer if needed." });
    }

    // ---------------- CALLBACK (webhook) ----------------
    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromBody] AnprCallback cb, CancellationToken ct)
    {
        var plate = ParkingService.NormalizePlate(cb.Plate);
        var raw = System.Text.Json.JsonSerializer.Serialize(cb);

        if (string.IsNullOrWhiteSpace(plate))
            return BadRequest("Missing/invalid plate.");

        if (cb.Direction.Equals("ENTRY", StringComparison.OrdinalIgnoreCase))
        {
            var (isMember, sessionId) = await _parking.HandleEntryAsync(plate, "WEBHOOK", raw, ct);
            return Ok(new { plate, isMember, sessionId });
        }

        if (cb.Direction.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
        {
            var result = await _parking.HandleExitAsync(plate, "WEBHOOK", raw, ct);
            return Ok(new { plate, result });
        }

        return BadRequest("Direction must be ENTRY or EXIT");
    }
}
