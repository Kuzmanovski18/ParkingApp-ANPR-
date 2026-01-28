namespace AnprParking.Api.Services.Pricing;

public class HourlyPricingService : IPricingService
{
    public decimal CalculateAmount(DateTime fromUtc, DateTime toUtc)
    {
        var minutes = (toUtc - fromUtc).TotalMinutes;
        if (minutes <= 0) return 0m;

        var hours = (int)Math.Ceiling(minutes / 60.0);
        return hours * 30m; // 30 ден по започнат час
    }
}
