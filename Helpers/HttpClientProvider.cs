namespace Webwonders.Extensions;

/// <summary>
/// Static HttpClientProvider. Makes sure only one Httpclient exists
/// </summary>
public class HttpClientProvider
{
    private static System.Net.Http.HttpClient _httpClient;

    public static System.Net.Http.HttpClient HttpClient { get { return _httpClient ??= new System.Net.Http.HttpClient(); } }
}
