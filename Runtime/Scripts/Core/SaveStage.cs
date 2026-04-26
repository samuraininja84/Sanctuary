namespace Sanctuary
{
    /// <summary>
    /// Controls the stage of the saving process. This is used in <see cref="ISaveStore"/> callbacks to allow stores to perform actions at different stages of the saving process.
    /// </summary>
    /// <remarks>Used internally by the save system to manage the saving process with Async operations.</remarks>
    public enum SaveStage
    {
        Prepare = 0,
        Commit = 1,
        PostCommit = 2
    }
}
