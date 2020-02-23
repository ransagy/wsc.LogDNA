using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using wsc.LogDNA;
using Xunit;

namespace UnitTests
{
    public class Tests
    {
        // TODO add mocked handler that auto replies if given correctly formed data
        private static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

        [Fact]
        public async Task HttpSingleLogOk()
        {
            IApiClient client = await InitConfigAndClient().ConfigureAwait(false);

            client.AddLine(new LogLine("MyLog", "From HTTP Client"));

            // TODO should be awaiting the mock result instead.
            Thread.Sleep((int)TimeSpan.FromSeconds(30).TotalMilliseconds);

            client.Disconnect();
        }

        [Fact]
        public async Task HttpMultipleLogsOk()
        {
            IApiClient client = await InitConfigAndClient().ConfigureAwait(false);

            for (int i = 0; i < 1000; i++)
            {
                client.AddLine(new LogLine("MyLog", $"From Default Client {i} {DateTime.UtcNow.ToShortTimeString()}"));
            }

            // TODO should be awaiting the mock result instead.
            Thread.Sleep((int)TimeSpan.FromSeconds(30).TotalMilliseconds);

            client.Disconnect();
        }

        private async Task<IApiClient> InitConfigAndClient()
        {
            var config = new ClientConfiguration("1082cb87a05595a2c997044cdbbefc4e") { Tags = new[] { "foo", "bar" }, LogInternalsToConsole = true };
            IApiClient client = new HttpApiClient(config, httpClient);
            await client.ConnectAsync().ConfigureAwait(false);
            return client;
        }
    }
}
