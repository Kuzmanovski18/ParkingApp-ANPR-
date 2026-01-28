using System.Text;
using AnprParking.Api.Domain.Entities;
using AnprParking.Api.Infrastructure;
using AnprParking.Api.Services;
using AnprParking.Api.Services.Auth;
using AnprParking.Api.Services.PlateRecognition;
using AnprParking.Api.Services.Pricing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//  Swagger + JWT Bearer button
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ANPR Parking API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {your JWT token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// ---------------- DB ----------------
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ---------------- App services ----------------
builder.Services.AddScoped<IPricingService, HourlyPricingService>();
builder.Services.AddScoped<ParkingService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddHttpClient<IPlateRecognizer, PlateRecognizerClient>();

builder.Services.AddHttpContextAccessor();

// ---------------- CORS (dev) ----------------
const string DevCors = "dev";
builder.Services.AddCors(o => o.AddPolicy(DevCors, p =>
{
    p.WithOrigins("http://localhost:5173", "https://localhost:5173")
     .AllowAnyHeader()
     .AllowAnyMethod();
}));

// ---------------- Identity (Users + Roles) ----------------
builder.Services
    .AddIdentityCore<ApplicationUser>(opt =>
    {
        opt.Password.RequireDigit = false;
        opt.Password.RequireUppercase = true;
        opt.Password.RequireLowercase = true;
        opt.Password.RequireNonAlphanumeric = true;
        opt.Password.RequiredLength = 8;

        opt.User.RequireUniqueEmail = true; // можеш true ако сакаш
    })
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders(); //  не е must, ама е добро

// ---------------- JWT Auth ----------------
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "anpr-parking";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "anpr-parking-client";
var jwtKey = builder.Configuration["Jwt:SigningKey"];

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Missing Jwt:SigningKey in configuration.");

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

// ---------------- Authorization ----------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ---------------- Stripe ----------------
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

var app = builder.Build();

//  Auto migrate (dev-friendly)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

//  Seed roles + default admin (најдобро само во Development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Roles
    foreach (var role in new[] { "User", "Admin" })
    {
        if (!await roleMgr.RoleExistsAsync(role))
        {
            var rr = await roleMgr.CreateAsync(new IdentityRole(role));
            if (!rr.Succeeded)
            {
                var msg = string.Join("; ", rr.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new Exception("Role seed failed: " + msg);
            }
        }
    }

    // Admin credentials from appsettings.json
    var adminUser = cfg["Seed:AdminUsername"] ?? "admin";
    var adminPass = cfg["Seed:AdminPassword"] ?? "Admin123!";
    var adminEmail = cfg["Seed:AdminEmail"] ?? "admin@anpr.local";

    var existingAdmin = await userMgr.FindByNameAsync(adminUser);

    if (existingAdmin is null)
    {
        var u = new ApplicationUser
        {
            UserName = adminUser,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var created = await userMgr.CreateAsync(u, adminPass);
        if (!created.Succeeded)
        {
            var msg = string.Join("; ", created.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new Exception("Admin seed failed: " + msg);
        }

        var addRole = await userMgr.AddToRoleAsync(u, "Admin");
        if (!addRole.Succeeded)
        {
            var msg = string.Join("; ", addRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new Exception("Admin role assign failed: " + msg);
        }
    }
    else
    {
        // ако постои, осигурај дека е Admin
        if (!await userMgr.IsInRoleAsync(existingAdmin, "Admin"))
        {
            var addRole = await userMgr.AddToRoleAsync(existingAdmin, "Admin");
            if (!addRole.Succeeded)
            {
                var msg = string.Join("; ", addRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new Exception("Admin role assign failed: " + msg);
            }
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(DevCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
