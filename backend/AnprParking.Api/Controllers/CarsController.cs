using AnprParking.Api.Domain.Entities;
using AnprParking.Api.Infrastructure;
using AnprParking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AnprParking.Api.Controllers;

public record AddCarRequest(string Plate, string? Label);

[Authorize]
[ApiController]
[Route("api/cars")]
public class CarsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CarsController(AppDbContext db) => _db = db;

    // GET /api/cars/my
    [HttpGet("my")]
    public async Task<ActionResult> My(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var cars = await _db.UserCars
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedUtc)
            .Select(c => new
            {
                id = c.Id,
                plate = c.Plate,
                label = c.Label,
                createdUtc = c.CreatedUtc
            })
            .ToListAsync(ct);

        return Ok(cars);
    }

    // POST /api/cars
    [HttpPost]
    public async Task<ActionResult> Add([FromBody] AddCarRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var plate = ParkingService.NormalizePlate(req.Plate);
        if (plate.Length < 5) return BadRequest("Invalid plate.");

        var exists = await _db.UserCars.AnyAsync(c => c.UserId == userId && c.Plate == plate, ct);
        if (exists) return Conflict("Car already exists.");

        var car = new UserCar
        {
            UserId = userId,
            Plate = plate,
            Label = string.IsNullOrWhiteSpace(req.Label) ? null : req.Label.Trim()
        };

        _db.UserCars.Add(car);
        await _db.SaveChangesAsync(ct);

        return Ok(new { id = car.Id, plate = car.Plate, label = car.Label, createdUtc = car.CreatedUtc });
    }

    // DELETE /api/cars/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var car = await _db.UserCars.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);
        if (car == null) return NotFound();

        _db.UserCars.Remove(car);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}
