namespace UkApprenticeships.Functions.Services;

public interface IFlux2Service
{
    Task<Stream> GenerateImageAsync(string prompt, int width = 1024, int height = 1024);
}
