using System.ComponentModel.DataAnnotations;

namespace AnprParking.Api.Domain.Entities;

public class AnprEventLog
{
    public Guid Id { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    // Stored normalized (uppercase)
    [MaxLength(32)]
    public string Plate { get; set; } = "";

    // ENTRY | EXIT | PAY | MEMBERSHIP
    [MaxLength(32)]
    public string EventType { get; set; } = "";

    // CAMERA | MANUAL | APP | STRIPE
    [MaxLength(32)]
    public string Source { get; set; } = "";

    // Optional: cameraId/device id
    [MaxLength(64)]
    public string? SourceRef { get; set; }

    // raw json or any info you want to keep
    public string RawPayload { get; set; } = "";
}
