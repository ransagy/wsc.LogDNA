using System.Threading.Tasks;

namespace wsc.LogDNA
{
    public interface IApiClient
    {
        /// <summary>
        /// Whether the client is connected to LogDNA.
        /// </summary>
        /// <returns><c>true</c> if connected; Otherwise, <c>false</c>.</returns>
        bool Connected { get; set; }

        /// <summary>
        /// Connects to the LogDNA servers using the specified configuration.
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Disconnects the client from the LogDNA servers.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends the specified message directly to the LogDNA servers without buffering.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>True if the message was transmitted successfully.</returns>
        Task<bool> Send(string message);

        /// <summary>
        /// Adds log data to the buffer to be sent when the flush interval happens.
        /// </summary>
        /// <param name="line">The line.</param>
        void AddLine(LogLine line);

        /// <summary>
        /// Forcefully flushes the log buffer.
        /// </summary>
        void Flush();
    }
}