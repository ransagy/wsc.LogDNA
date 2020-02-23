using System;
using System.Collections.Generic;

namespace wsc.LogDNA
{
    public class ClientConfiguration
    {
        public ClientConfiguration(string ingestionIngestionKey)
        {
            IngestionKey = ingestionIngestionKey;
        }

        /// <summary>
        /// Gets or sets the delay before reconnecting after an authentication failure (in milliseconds).
        /// </summary>
        /// <value>
        /// The authentication fail delay. Defaults to 15 minutes.
        /// </value>
        public TimeSpan AuthFailDelay { get; set; } = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Gets or sets the period of time after which a flush should happen automatically.
        /// </summary>
        /// <value>
        /// The flush interval. Defaults to 250ms.
        /// </value>
        public TimeSpan FlushInterval { get; set; } = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Gets or sets the LogDNA key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string IngestionKey { get; set; }

        /// <summary>
        /// Gets or sets the tags used for dynamic grouping.
        /// </summary>
        /// <value>
        /// The tags.
        /// </value>
        public IEnumerable<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Log the internal operations of the LogDNA client to the Console window.
        /// </summary>
        /// <returns></returns>
        public bool LogInternalsToConsole { get; set; }

        /// <summary>
        /// Gets or sets the amount of times to retry a failing operation. Defaults to 10.
        /// </summary>
        /// <value>
        /// The total number of times to retry an operation that previously failed.
        /// </value>
        public int RetryAttempts { get; set; } = 10;

        /// <summary>
        /// Gets or sets the time to wait (in ms) before retrying a send operation.
        /// </summary>
        /// <value>
        /// The time to wait (in ms) before retrying a send operation.
        /// </value>
        public TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMilliseconds(5_000);

        /// <summary>
        /// Gets the name of the host. By default, this is the machine name, but it can be overridden.
        /// </summary>
        /// <value>
        /// The host name.
        /// </value>
        public string HostName { get; set; } = Environment.MachineName;
    }
}
