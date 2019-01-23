using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharpBird.Exceptions;

namespace SharpBird.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<T> Get<T>(this HttpClient httpClient, string uri, Dictionary<string, string> query = null)
        {
            var strQuery = query?.Count > 0 ? "?" +
                string.Join("&", query.Select(x => $"{x.Key}={x.Value}")) : string.Empty;

            using (var response = await httpClient.GetAsync(uri + strQuery))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new BlacklistedException();
                    case HttpStatusCode.ProxyAuthenticationRequired:
                        throw new ProxyAuthenticationRequiredException();
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }
    }
}
