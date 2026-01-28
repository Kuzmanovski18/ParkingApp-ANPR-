using Microsoft.AspNetCore.Identity;

namespace AnprParking.Api.Domain.Entities;
public class ApplicationUser : IdentityUser
{
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
