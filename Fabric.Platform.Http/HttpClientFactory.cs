using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Fabric.Platform.Shared;

namespace Fabric.Platform.Http
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly string _correlationToken;
        private readonly string _subject;
        private readonly TokenClient _tokenClient;

        public HttpClientFactory(string tokenUrl, string clientId, string secret, string correlationToken, string subject)
        {
            _correlationToken = correlationToken;
            _subject = subject;
            _tokenClient = new TokenClient(tokenUrl, clientId, secret);
        }
        public async Task<HttpClient> Create(Uri uri, string requestScope)
        {
            var response = await _tokenClient.RequestClientCredentialsAsync(requestScope).ConfigureAwait(false);
            return CreateWithAccessToken(uri, response.AccessToken);
        }

        public HttpClient CreateWithAccessToken(Uri uri, string accessToken)
        {
            var client = new HttpClient { BaseAddress = uri };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Add(Constants.FabricHeaders.CorrelationTokenHeaderName, _correlationToken);
            if (!string.IsNullOrEmpty(_subject))
            {
                client.DefaultRequestHeaders.Add(Constants.FabricHeaders.SubjectNameHeader, _subject);
            }
            return client;
        }
    }
}