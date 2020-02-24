using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wsc.LogDNA;

namespace LogDNA.Tests
{
    internal static class Setup
    {
        private static readonly string baseURI = $"https://{nameof(LogDNA)}.{nameof(LogDNA.Tests)}.{nameof(Setup)}";

        internal const string FakeServerHost = "fake.logs.server.com";
        internal const string MockedMethodName = "SendAsync";
        internal const string AuthenticationURIPath = "authenticate";

        internal static async Task<(IApiClient, Mock<HttpMessageHandler>)> InitConfigAndClient()
        {
            var mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClientMock = new HttpClient(mock.Object) { BaseAddress = new Uri(baseURI) };

            var config = new ClientConfiguration("DummyKey") { Tags = new[] { "foo", "bar" }, LogInternalsToConsole = true };
            IApiClient client = new HttpApiClient(config, httpClientMock);

            var mockAuthResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent($"{{ \"server\": \"{FakeServerHost}\", \"port\": 443, \"ssl\": true, \"token\": \"dummy:token\", \"transport\": \"http\", \"status\": \"ok\" }}", Encoding.UTF8, "application/json") };

            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(MockedMethodName, ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsoluteUri.Contains(AuthenticationURIPath, StringComparison.OrdinalIgnoreCase)), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockAuthResponse)
                .Verifiable();

            await client.ConnectAsync().ConfigureAwait(false);

            return (client, mock);
        }
    }
}
