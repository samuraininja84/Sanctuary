using System.Threading.Tasks;
using Sanctuary;
using Sanctuary.Loaders;

namespace Sanctaury.Samples
{
    ///// <summary>
    ///// A dummy save loader that does nothing.
    ///// </summary>
    //public class DummySaveLoader : ISaveLoader
    //{
    //    private bool _exists;

    //    public Task<string> GetName() => Task.FromResult("Dummy Save");

    //    public Task<bool> Exists() => Task.FromResult(_exists);

    //    public Task<ISaveData> Load() => Task.FromResult<ISaveData>(new SaveData());

    //    public Task Save(ISaveData data) => Task.CompletedTask;

    //    public Task<ISaveData> Create()
    //    {
    //        // Mark the save as existing.
    //        _exists = true;

    //        // Create a new, empty save data.
    //        return Task.FromResult<ISaveData>(new SaveData());
    //    }

    //    public Task Delete()
    //    {
    //        // Mark the save as not existing.
    //        _exists = false;

    //        // Return a completed task.
    //        return Task.CompletedTask;
    //    }

    //    public void SetDirectory(string subdirectory) { }
    //}
}

