using System.Threading.Tasks;

namespace Sanctuary
{
    public interface ISanctuaryService
    {
        Task<SaveResult> SaveAsync<T>(string slotId, T data) where T : class;

        Task<LoadResult<T>> LoadAsync<T>(string slotId) where T : class;

        SaveSlotInfo[] GetAvailableSlots();

        SaveSlotInfo GetSlot(string slotId);

        Task<bool> DeleteSlotAsync(string slotId);

        void RegisterMigrationStep(ISaveMigrationStep step);
    }
}