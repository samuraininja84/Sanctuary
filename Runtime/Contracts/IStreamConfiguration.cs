using System.IO;
using System.Threading.Tasks;
using Sanctuary.Configuration;

namespace Sanctuary
{
    public interface IStreamConfiguration
    {
        /// <summary>
        /// Filesystem root the file provider writes save files under.
        /// </summary>
        string RootPath { get; }

        /// <summary>
        /// Schema version stamped onto new save envelopes. Bump when your save format changes.
        /// </summary>
        int CurrentSchemaVersion { get; }

        /// <summary>
        /// Gets a stream based on the specified stream type and optional file path.
        /// </summary>
        /// <param name="streamType">The type of stream to get.</param>
        /// <param name="filePath">The optional file path for the stream. If null, a default path is used.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the requested stream.</returns>
        Task<Stream> GetStream(StreamType streamType, string filePath = null);
    }
}