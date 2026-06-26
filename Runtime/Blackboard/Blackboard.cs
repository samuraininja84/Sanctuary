using System;
using System.Collections.Generic;

namespace Sanctuary.Blackboard
{
    [Serializable]
    public class Blackboard
    {
        readonly Dictionary<string, BlackboardKey> keyRegistry = new();
        readonly Dictionary<BlackboardKey, object> entries = new();

        public List<Action> PassedActions { get; } = new();

        public void RegisterAction(Action action)
        {
            // Validate that the action is not null before adding it to the PassedActions list.
            Preconditions.CheckNotNull(action);

            // Add the action to the PassedActions list.
            PassedActions.Add(action);
        }

        public void UnregisterAction(Action action)
        {
            // Validate that the action is not null before attempting to remove it from the PassedActions list.
            Preconditions.CheckNotNull(action);

            // Remove the action from the PassedActions list.
            PassedActions.Remove(action);
        }

        public void ClearActions() => PassedActions.Clear();

        public bool TryGetValue<T>(BlackboardKey key, out T value)
        {
            // Check if the key exists in the entries dictionary and if the entry is of type BlackboardEntry<T>.
            if (entries.TryGetValue(key, out var entry) && entry is BlackboardEntry<T> castedEntry)
            {
                // If the key exists and the type matches, set the out parameter to the value of the entry.
                value = castedEntry.Value;

                // Return true to indicate that the value was successfully retrieved.
                return true;
            }

            // Set the out parameter to the default value of T if the key does not exist or the type does not match.
            value = default;

            // Return false to indicate that the value could not be retrieved.
            return false;
        }

        public void SetValue<T>(BlackboardKey key, T value) => entries[key] = BlackboardEntry<T>.Create(key, value);

        public BlackboardKey GetOrAddKey(string keyName)
        {
            // Validate that the keyName is not null.
            Preconditions.CheckNotNull(keyName);

            // Check if the key already exists in the registry.
            if (!keyRegistry.TryGetValue(keyName, out BlackboardKey key))
            {
                // Create a new key if it doesn't exist in the registry.
                key = BlackboardKey.Create(keyName);

                // Add the new key to the registry.
                keyRegistry[keyName] = key;
            }

            // Return the existing key if it was found, or the newly created key if it was added.
            return key;
        }

        public bool ContainsKey(BlackboardKey key) => entries.ContainsKey(key);

        public void Remove(BlackboardKey key) => entries.Remove(key);

        public void ReadAllEntries()
        {
            // Iterate through all entries in the blackboard.
            foreach (var entry in entries)
            {
                // Get the type of the entry's value.
                var entryType = entry.Value.GetType();

                // Check if the entry is of type BlackboardEntry<T> using reflection.
                if (entryType.IsGenericType && entryType.GetGenericTypeDefinition() == typeof(BlackboardEntry<>))
                {
                    // Get the Value property of the BlackboardEntry<T> type using reflection.
                    var valueProperty = entryType.GetProperty("Value");

                    // Check if the valueProperty is null before attempting to get its value.
                    if (valueProperty == null) continue;

                    // Get the value of the entry using reflection.
                    var value = valueProperty.GetValue(entry.Value);

                    // Log the key and value to the Unity console for debugging purposes.
                    UnityEngine.Debug.Log($"Key: {entry.Key}, Value: {value}");
                }
            }
        }
    }
}
