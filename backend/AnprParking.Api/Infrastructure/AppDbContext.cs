using AnprParking.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AnprParking.Api.Infrastructure;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<MemberVehicle> MemberVehicles => Set<MemberVehicle>();
    public DbSet<UserCar> UserCars => Set<UserCar>();
    public DbSet<ParkingSession> ParkingSessions => Set<ParkingSession>();
    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
    public DbSet<AnprEventLog> AnprEventLogs => Set<AnprEventLog>();

    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Identity tables
        base.OnModelCreating(b);

        // ---------------- MemberVehicle (membership) ----------------
        b.Entity<MemberVehicle>(e =>
        {
            e.HasKey(x => x.Id);

            // Unique plate globally (membership is global)
            e.HasIndex(x => x.Plate).IsUnique();

            // helpful index if you filter by user
            e.HasIndex(x => x.UserId);
        });

        // ---------------- UserCar (my cars) ----------------
        b.Entity<UserCar>(e =>
        {
            e.HasKey(x => x.Id);

            // One user cannot add same plate twice
            e.HasIndex(x => new { x.UserId, x.Plate }).IsUnique();

            // quick lookup by plate
            e.HasIndex(x => x.Plate);

            // Optional FK to Identity user (ако UserCar има navigation)
            // e.HasOne<ApplicationUser>()
            //  .WithMany()
            //  .HasForeignKey(x => x.UserId)
            //  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------------- ParkingSession ----------------
        b.Entity<ParkingSession>(e =>
        {
            e.HasKey(x => x.Id);

            // frequent filtering: plate + status
            e.HasIndex(x => new { x.Plate, x.Status });

            // helpful if you query per user
            e.HasIndex(x => x.UserId);

            e.HasIndex(x => x.EntryUtc);
        });

        // ---------------- PaymentRecord ----------------
        b.Entity<PaymentRecord>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasIndex(x => x.Plate);
            e.HasIndex(x => x.PaidAtUtc);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => new { x.Kind, x.PaidAtUtc });
        });

        // ---------------- AnprEventLog ----------------
        b.Entity<AnprEventLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CreatedUtc);
            e.HasIndex(x => x.Plate);
        });
    }
}
