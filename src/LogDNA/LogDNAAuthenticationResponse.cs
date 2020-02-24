namespace wsc.LogDNA
{
    public class LogDNAAuthenticationResponse
    {
        /// <summary>
        /// The URL of the API server that should be used for future authentication attempts.
        /// </summary>
        public string ApiServer { get; set; }

        /// <summary>
        /// Gets or sets the authentication token received from LogDNA.
        /// </summary>
        /// <value>
        /// The authentication token.
        /// </value>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets hostname of the log server.
        /// </summary>
        /// <value>
        /// The log server.
        /// </value>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the port number of the log server.
        /// </summary>
        /// <value>
        /// The log server port.
        /// </value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an SSL connection to the log server should be used.
        /// </summary>
        /// <value>
        ///   <c>true</c> to use a secure socket connection; otherwise, <c>false</c>.
        /// </value>
        public bool Ssl { get; set; }
    }
}