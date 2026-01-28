namespace AnprParking.Api.Services.PlateRecognition;

public interface IPlateRecognizer
{
    Task<string> RecognizePlateAsync(IFormFile image, CancellationToken ct);
}
