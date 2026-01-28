namespace AnprParking.Api.Services.Pricing;

public interface IPricingService
{
    decimal CalculateAmount(DateTime fromUtc, DateTime toUtc);
}
