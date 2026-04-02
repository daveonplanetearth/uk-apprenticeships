using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UkApprenticeships.Functions.Configuration;

namespace UkApprenticeships.Functions.Services;

public class XApiService : IXApiService
{
    private readonly HttpClient _httpClient;
    private readonly XApiOptions _options;

    public XApiService(HttpClient httpClient, IOptions<XApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task PostTweetAsync(string text)
    {
        const string url = "https://api.x.com/2/tweets";

        var payload = JsonSerializer.Serialize(new { text });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
        request.Headers.TryAddWithoutValidation("Authorization", BuildOAuthHeader(url));
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task PostTweetMediaAsync(string text, Stream imageStream, string mediaType = "image/jpeg")
    {
        const string uploadUrl = "https://api.x.com/2/media/upload";

        var imageBytes = new MemoryStream();
        await imageStream.CopyToAsync(imageBytes);

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(uploadUrl));
        uploadRequest.Headers.TryAddWithoutValidation("Authorization", BuildOAuthHeader(uploadUrl));

        var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent(imageBytes.ToArray()), "media", "media");
        multipart.Add(new StringContent("tweet_image"), "media_category");
        uploadRequest.Content = multipart;

        var uploadResponse = await _httpClient.SendAsync(uploadRequest);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
        using var uploadDoc = JsonDocument.Parse(uploadJson);
        var mediaId = uploadDoc.RootElement.GetProperty("data").GetProperty("id").GetString();

        const string tweetUrl = "https://api.x.com/2/tweets";
        var payload = JsonSerializer.Serialize(new
        {
            text,
            media = new { media_ids = new[] { mediaId } }
        });

        using var tweetRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(tweetUrl));
        tweetRequest.Headers.TryAddWithoutValidation("Authorization", BuildOAuthHeader(tweetUrl));
        tweetRequest.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        var tweetResponse = await _httpClient.SendAsync(tweetRequest);
        tweetResponse.EnsureSuccessStatusCode();
    }

    private string BuildOAuthHeader(string url)
    {
        var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+", "").Replace("/", "").Replace("=", "");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var oauthParams = new SortedDictionary<string, string>
        {
            ["oauth_consumer_key"] = _options.ConsumerKey,
            ["oauth_nonce"] = nonce,
            ["oauth_signature_method"] = "HMAC-SHA1",
            ["oauth_timestamp"] = timestamp,
            ["oauth_token"] = _options.AccessToken,
            ["oauth_version"] = "1.0"
        };

        var paramString = string.Join("&", oauthParams.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        var baseString = $"POST&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramString)}";

        var signingKey = $"{Uri.EscapeDataString(_options.ConsumerSecret)}&{Uri.EscapeDataString(_options.AccessTokenSecret)}";
        using var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(baseString)));

        oauthParams["oauth_signature"] = signature;

        var headerValue = "OAuth " + string.Join(", ", oauthParams.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}=\"{Uri.EscapeDataString(p.Value)}\""));

        return headerValue;
    }
}
