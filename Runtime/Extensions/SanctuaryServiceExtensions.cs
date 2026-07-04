using System;
using System.Threading.Tasks;

namespace Sanctuary.Extensions
{
    public static class SanctuaryServiceExtensions
    {
        public static async Task<DateTime> LastModifiedTime(this ISanctuaryService service, string slotId)
        {
            // Get the slot information for the specified slot ID
            var slot = service.GetSlot(slotId) ?? throw new InvalidOperationException($"Slot with ID '{slotId}' does not exist.");

            // Return the last modified time of the specified slot
            return slot.LastSaveTime;
        }

        public static async Task<double> PlayTime(this ISanctuaryService service, string slotId)
        {
            // Get the slot information for the specified slot ID
            var slot = service.GetSlot(slotId) ?? throw new InvalidOperationException($"Slot with ID '{slotId}' does not exist.");

            // Return the total playtime in seconds
            return slot.TotalPlayTimeSeconds;
        }

        public static async Task DeleteAllAsync(this ISanctuaryService service)
        {
            // Get all available slots from the service
            var slots = service.GetAvailableSlots();

            // Iterate through each slot and delete it
            for (int i = 0; i < slots.Length; i++) await service.DeleteAsync(slots[i].SlotId);
        }
    }
}