namespace UkApprenticeships.Functions.Services;

public interface IXApiService
{
    Task PostTweetAsync(string text);
    Task PostTweetMediaAsync(string text, Stream imageStream, string mediaType = "image/jpeg");
}
