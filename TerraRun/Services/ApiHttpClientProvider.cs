namespace TerraRun.Services;

internal static class ApiHttpClientProvider
{
    private static readonly Lazy<HttpClient> Client = new(() => new HttpClient
    {
        BaseAddress = new Uri("http://10.0.2.2:5000/api/")
    });

    public static HttpClient Instance => Client.Value;
}
