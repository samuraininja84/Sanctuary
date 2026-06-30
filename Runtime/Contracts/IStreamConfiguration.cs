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
    }
}