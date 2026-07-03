using System.Threading.Tasks;

namespace Sanctuary.Extensions
{
    public static class SanctuaryServiceExtensions
    {
        public static async Task DeleteAllAsync(this ISanctuaryService service)
        {
            // Get all available slots from the service
            var slots = service.GetAvailableSlots();

            // Iterate through each slot and delete it
            for (int i = 0; i < slots.Length; i++) await service.DeleteAsync(slots[i].SlotId);
        }
    }
}