using System.Net.Http;
using System.Threading.Tasks;

namespace CreeperX.Requests;

public static class HttpRequestHelper
{
    private static readonly HttpClient client = new();

    public static async Task<HttpResponseMessage> GetUri(string uri)
    {
        return await client.GetAsync(uri);
    }
}
