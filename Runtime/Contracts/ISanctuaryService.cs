using System.Threading.Tasks;

namespace Sanctuary
{
    public interface ISanctuaryService
    {
        void RegisterMigrationStep(ISaveMigrationStep step);

        Task<SaveResult> SaveAsync<T>(string slotId, T data) where T : class;

        Task<LoadResult<T>> LoadAsync<T>(string slotId) where T : class;

        Task<bool> DeleteAsync(string slotId);

        Task<bool> ExistsAsync(string slotId);

        SaveSlotInfo GetSlot(string slotId);

        SaveSlotInfo[] GetAvailableSlots();
    }
}