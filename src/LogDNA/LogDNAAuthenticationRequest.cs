using System;
using System.Reflection;

namespace wsc.LogDNA
{
    public class LogDNAAuthenticationRequest
    {
        /// <summary>
        /// Gets or set the comma-separated string of tags.
        /// </summary>
        /// <value>
        /// All tags.
        /// </value>
        public string Tags { get; set; } = "";

        /// <summary>
        /// Gets the name of the host. By default, this is the machine name, but it can be overridden.
        /// </summary>
        /// <value>
        /// The host name.
        /// </value>
        public string HostName { get; set; } = Environment.MachineName;

        /// <summary>
        /// Gets the name of this agent.
        /// </summary>
        /// <value>
        /// The assembly name.
        /// </value>
        public string AgentName => Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// Gets the agent version.
        /// </summary>
        /// <value>
        /// The agent version.
        /// </value>
        public string AgentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Gets the Operating System ID.
        /// </summary>
        /// <value>
        /// Full name string for the running OS.
        /// </value>
        public string OsDist => Environment.OSVersion.ToString();
    }
}