using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sanctuary
{
    public sealed class SaveSlotRegistry
    {
        private readonly Dictionary<string, SaveSlotInfo> m_Slots = new();

        public SaveSlotInfo this[string slotId] => GetSlot(slotId);

        public int Count => m_Slots.Count;

        public SaveSlotInfo GetSlot(string slotId) => m_Slots.TryGetValue(slotId, out var info) ? info : null;

        public SaveSlotInfo[] GetAllSlots() => m_Slots.Values.ToArray();

        public bool HasSlot(string slotId) => m_Slots.ContainsKey(slotId);

        public void RegisterSlot(string slotId, SaveSlotInfo info) => m_Slots[slotId] = info;

        public void UpdateSlot(string slotId, SaveSlotInfo info) => m_Slots[slotId] = info;

        public void RemoveSlot(string slotId) => m_Slots.Remove(slotId);

        public byte[] ToBytes() => System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(m_Slots));

        public void FromBytes(byte[] data)
        {
            // If the input data is null or empty, clear the internal dictionary and return.
            if (data == null || data.Length == 0)
            {
                m_Slots.Clear();
                return;
            }

            // Convert the byte array to a JSON string using UTF-8 encoding.
            var json = System.Text.Encoding.UTF8.GetString(data);

            // Deserialize the JSON string into a dictionary of save slot information.
            var slots = JsonConvert.DeserializeObject<Dictionary<string, SaveSlotInfo>>(json);

            // If the deserialization was successful and the slots dictionary is not null, populate the internal dictionary with the deserialized slots.
            if (slots != null)
            {
                m_Slots.Clear();
                foreach (var kvp in slots)
                {
                    m_Slots[kvp.Key] = kvp.Value;
                }
            }
        }

        public static SaveSlotRegistry CreateFromBytes(byte[] data)
        {
            // Initialize a new instance of SaveSlotRegistry to hold the deserialized data.
            var registry = new SaveSlotRegistry();

            // If the input data is null or empty, return the empty registry.
            if (data == null || data.Length == 0) return registry;

            // Convert the byte array to a JSON string using UTF-8 encoding.
            var json = System.Text.Encoding.UTF8.GetString(data);

            // Deserialize the JSON string into a dictionary of save slot information.
            var slots = JsonConvert.DeserializeObject<Dictionary<string, SaveSlotInfo>>(json);

            // If the deserialization was successful and the slots dictionary is not null, populate the registry with the deserialized slots.
            if (slots != null)
            {
                // Iterate through each key-value pair in the slots dictionary and add them to the registry's internal dictionary.
                foreach (var kvp in slots)
                {
                    // Add the slot information to the registry's internal dictionary using the slot ID as the key.
                    registry.m_Slots[kvp.Key] = kvp.Value;
                }
            }

            // Return the populated registry instance.
            return registry;
        }
    }
}
