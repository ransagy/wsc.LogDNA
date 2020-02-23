# wsc.LogDNA
.NET Standard 2.0 client for the LogDNA service.

Based off of the work done by Red Bear Software @ https://github.com/RedBearSys/RedBear.LogDNA

```
Install-Package wsc.LogDNA
```

Allows log data to be sent to LogDNA using managed code.

```c#
private const string IngestionKey = "PUT-KEY-HERE";

public static void Main()
{
  var config = new ClientConfiguration(IngestionKey) { Tags = new[] { "foo", "bar" } };
  IApiClient client = new HttpApiClient(config, httpClient);
  await client.ConnectAsync().ConfigureAwait(false);

  client.AddLine(new LogLine("MyLog", "From Default Client"));

  client.Flush();
  client.Disconnect();
}
```
