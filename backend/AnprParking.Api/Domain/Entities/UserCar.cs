using System.ComponentModel.DataAnnotations;

namespace AnprParking.Api.Domain.Entities;

public class UserCar
{
    public Guid Id { get; set; }

    // FK to AspNetUsers.Id
    [Required]
    public string UserId { get; set; } = default!;

    // Stored normalized (uppercase, no spaces)
    [Required]
    [MaxLength(32)]
    public string Plate { get; set; } = default!;

    // Optional user-friendly name: "Tesla", "Family car", etc.
    [MaxLength(64)]
    public string? Label { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
