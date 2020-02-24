using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace wsc.LogDNA
{
    public partial class HttpApiClient : IApiClient
    {
        private const string DefaultLogDNAHost = "api.logdna.com";

        private readonly ConcurrentQueue<LogLine> _buffer = new ConcurrentQueue<LogLine>();
        private readonly ConcurrentDictionary<string, int> _flags = new ConcurrentDictionary<string, int>();
        private readonly JsonSerializerSettings authJsonSettings = new JsonSerializerSettings { ContractResolver = new LowercaseContractResolver() };
        private readonly HttpClient client;

        private readonly Timer timer;

        internal ClientConfiguration Configuration { get; set; }

        internal LogDNAAuthenticationRequest AuthConfiguration { get; set; }

        internal LogDNAAuthenticationResponse AuthResult { get; set; }

        public bool Connected { get; set; }

        public HttpApiClient(ClientConfiguration clientConfiguration, HttpClient client)
        {
            this.client = client;
            Configuration = clientConfiguration;

            timer = new Timer(Configuration.FlushInterval.TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;
        }

        public async Task ConnectAsync()
        {
            timer?.Stop();

            AuthConfiguration = new LogDNAAuthenticationRequest { Tags = string.Join(",", Configuration.Tags), HostName = Configuration.HostName };

            HttpStatusCode status = HttpStatusCode.Unused;
            int tries = 0;

            while (status != HttpStatusCode.OK && tries < 10)
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, $"https://{AuthResult?.ApiServer ?? DefaultLogDNAHost}/authenticate/{Configuration.IngestionKey}"))
                {
                    request.Headers.Add("User-Agent", $"{AuthConfiguration.AgentName}/{AuthConfiguration.AgentVersion}");
                    request.Content = new StringContent(JsonConvert.SerializeObject(AuthConfiguration, authJsonSettings), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    AuthResult = JsonConvert.DeserializeObject<LogDNAAuthenticationResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    status = response.StatusCode;

                    if (status != HttpStatusCode.OK)
                    {
                        InternalLogger("Auth failed; Connection will be retried after a delay.");
                        Thread.Sleep(Configuration.AuthFailDelay);
                        tries++;
                    }

                    if (status == HttpStatusCode.OK && !string.IsNullOrWhiteSpace(AuthResult?.ApiServer) && !AuthResult.ApiServer.Equals(DefaultLogDNAHost, StringComparison.OrdinalIgnoreCase) && AuthResult.Ssl)
                    {
                        status = HttpStatusCode.Unused;
                    }
                    else
                    {
                        AuthResult.ApiServer ??= DefaultLogDNAHost;
                    }
                }
            }

            if (status == HttpStatusCode.OK)
            {
                Connected = true;

                timer.Start();
            }
        }

        public void Disconnect()
        {
            InternalLogger("Disconnecting..");
            Flush();
            timer.Stop();
            Connected = false;
        }

        public async Task<bool> Send(string message)
        {
            InternalLogger($"Sending message: \"{message}\"..");

            if (string.IsNullOrEmpty(AuthResult.Server))
            {
                Reconnect();
            }

            if (string.IsNullOrEmpty(AuthResult.Server))
            {
                return false;
            }

            string requestUri = $"http{(AuthResult.Ssl ? "s" : string.Empty)}://{AuthResult.Server}:{AuthResult.Port}/logs/agent?timestamp={DateTime.Now.ToJavaTimestamp()}&hostname={Configuration.HostName}&mac=&ip=&tags={string.Join(",", Configuration.Tags)}&compress=1";

            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                byte[] authBytes = Encoding.ASCII.GetBytes($"x:{Configuration.IngestionKey}");

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
                request.Headers.Add("User-Agent", $"{AuthConfiguration.AgentName}/{AuthConfiguration.AgentVersion}");
                request.Headers.Add("Connection", "keep-alive");
                request.Headers.Add("Keep-Alive", "60000");

                byte[] jsonBytes = Encoding.UTF8.GetBytes(message);

                var payloadMemoryStream = new MemoryStream();

                using (var gzip = new GZipStream(payloadMemoryStream, CompressionMode.Compress, true))
                {
                    gzip.Write(jsonBytes, 0, jsonBytes.Length);
                }

                payloadMemoryStream.Position = 0;

                var content = new StreamContent(payloadMemoryStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                content.Headers.ContentEncoding.Add("gzip");
                request.Content = content;

                using (HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        InternalLogger($"Received HTTP Response: {response.StatusCode}");

                        if (response.StatusCode == HttpStatusCode.RequestTimeout || response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            Thread.Sleep(Configuration.RetryTimeout);
                            return await Send(message).ConfigureAwait(false);
                        }

                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            Reconnect();
                            return await Send(message).ConfigureAwait(false);
                        }
                    }
                }
            }

            return true;
        }

        private async void Reconnect()
        {
            try
            {
                if (StartReconnect())
                {
                    await ConnectAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                EndReconnect();
            }
        }

        private bool StartReconnect()
        {
            return _flags.AddOrUpdate("reconnect", 1, (_, i) => i + 1) == 1;
        }

        private void EndReconnect()
        {
            _flags.TryRemove("reconnect", out int _);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Flush();
        }

        public void AddLine(LogLine line)
        {
            _buffer.Enqueue(line);
        }

        public async void Flush()
        {
            if (StartFlush())
            {
                try
                {
                    int length = _buffer.Count;
                    var items = new List<LogLine>();

                    for (int i = 0; i < length; i++)
                    {
                        _buffer.TryDequeue(out LogLine line);

                        if (line == null)
                        {
                            break;
                        }

                        items.Add(line);
                    }

                    if (items.Count > 0)
                    {
                        var message = new BufferMessage { LogLines = items };

                        try
                        {
                            await Send(JsonConvert.SerializeObject(message)).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            InternalLogger($"Failed to send message to LogDNA servers:{Environment.NewLine}{ex}");
                        }
                    }
                }
                finally
                {
                    EndFlush();
                }
            }
        }

        private bool StartFlush()
        {
            return _flags.AddOrUpdate("flushing", 1, (_, i) => i + 1) == 1;
        }

        private void EndFlush()
        {
            _flags.TryRemove("flushing", out int _);
        }

        private void InternalLogger(string message)
        {
            if (Configuration.LogInternalsToConsole)
            {
                Console.WriteLine(message);
            }
        }
    }
}
