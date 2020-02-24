using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wsc.LogDNA;
using Xunit;

namespace UnitTests
{
    public class Tests
    {
        [Fact]
        public async Task AuthenticationOk()
        {
            (IApiClient client, Mock<HttpMessageHandler> mock) = await Setup.InitConfigAndClient().ConfigureAwait(false);

            mock.Protected().Verify(Setup.MockedMethodName, Times.Once(), ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsoluteUri.Contains(Setup.AuthenticationURIPath, StringComparison.OrdinalIgnoreCase)), ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1000)]
        public async Task AddLogLineOk(int runCount)
        {
            (IApiClient client, Mock<HttpMessageHandler> mock) = await Setup.InitConfigAndClient().ConfigureAwait(false);
            var handle = new ManualResetEventSlim();

            var mockResponse = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(Setup.MockedMethodName, ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsoluteUri.Contains(Setup.FakeServerHost, StringComparison.OrdinalIgnoreCase)), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse)
                .Callback(() => handle.Set())
                .Verifiable();

            for (int i = 0; i < runCount; i++)
            {
                client.AddLine(new LogLine("MyLog", $"Client Test {i} {DateTime.UtcNow.ToShortTimeString()}"));
            }

            handle.Wait((int)TimeSpan.FromMinutes(1).TotalMilliseconds);

            mock.Protected().Verify(Setup.MockedMethodName, Times.AtLeastOnce(), ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsoluteUri.Contains(Setup.FakeServerHost, StringComparison.OrdinalIgnoreCase)), ItExpr.IsAny<CancellationToken>());

            client.Disconnect();
        }
    }
}
