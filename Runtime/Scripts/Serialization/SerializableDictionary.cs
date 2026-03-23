using System.Collections.Generic;
using UnityEngine;

namespace Sanctuary
{
    /// <summary>
    /// Represents a dictionary that can be serialized and deserialized, enabling storage of key-value pairs in Unity's serialization system.
    /// </summary>
    /// <remarks>
    /// The <see cref="SerializableDictionary{TKey, TValue}"/> class extends the standard <see cref="Dictionary{TKey, TValue}"/> and implements <see cref="ISerializationCallbackReceiver"/> to support serialization in Unity. 
    /// During serialization, keys and values are stored in separate lists to ensure compatibility with Unity's serialization system. 
    /// This class is particularly useful for scenarios where dictionaries need to be serialized in Unity, such as saving game data or configuring serialized objects.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the dictionary. Keys must be serializable and unique within the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary. Values must be serializable.</typeparam>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public SerializableDictionary() { }

        public SerializableDictionary(Dictionary<TKey, TValue> dictionary)
        {
            // Initialize the keys and values lists with the keys and values from the provided dictionary.
            keys = new List<TKey>(dictionary.Keys);
            values = new List<TValue>(dictionary.Values);
        }

        public void OnBeforeSerialize()
        {
            // Clear the existing contents of the keys and values lists before serialization.
            keys.Clear();
            values.Clear();

            // Iterate through each key-value pair in the dictionary and add them to the respective lists.
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            // Clear the current contents of the dictionary before deserialization.
            this.Clear();

            // Check if the number of keys matches the number of values.
            if (keys.Count != values.Count)
            {
                // Log an error if the number of keys does not match the number of values.
                Debug.LogError("Tried to deserialize a SerializableDictionary, but the amount of keys(" + keys.Count + ") does not match the number of values (" + values.Count + ") which indicates that something went wrong.");

                // Exit the method early to prevent further processing.
                return;
            }

            // Add each key-value pair back into the dictionary after deserialization.
            for (int i = 0; i < keys.Count; i++) this.Add(keys[i], values[i]);
        }

        /// <summary>
        /// Adds a key-value pair to the collection. If the key already exists, updates the associated value.
        /// </summary>
        /// <remarks>If the specified <paramref name="key"/> already exists in the collection, the
        /// existing value is replaced with the provided <paramref name="value"/>. Otherwise, the key-value pair is
        /// added to the collection.</remarks>
        /// <param name="key">The key to add or update in the collection. Cannot be <see langword="null"/>.</param>
        /// <param name="value">The value to associate with the specified key.</param>
        public void AddPair(TKey key, TValue value)
        {
            // Check if the key is null to prevent adding null keys to the dictionary.
            if (ContainsKey(key))
            {
                // Update the existing key-value pair with the new value.
                UpdatePair(key, value);

                // Exit the method early since the key already exists.
                return;
            }

            /// If the key does not exist, add the new key-value pair to the dictionary.
            Add(key, value);
        }

        /// <summary>
        /// Updates the value associated with the specified key in the dictionary.
        /// </summary>
        /// <param name="key">The key of the pair to update.</param>
        /// <param name="value">The new value to associate with the specified key.</param>
        public void UpdatePair(TKey key, TValue value) => this[key] = value;

        /// <summary>
        /// Removes the key-value pair with the specified key from the dictionary.
        /// </summary>
        /// <remarks>If the specified key does not exist in the dictionary, an error message is logged,
        /// and no changes are made.</remarks>
        /// <param name="key">The key of the pair to remove.</param>
        public void RemovePair(TKey key)
        {
            // Check if the key exists in the dictionary before attempting to remove it.
            if (!ContainsKey(key))
            {
                // Log an error message indicating that the key does not exist.
                Debug.LogError("Tried to remove a key from a SerializableDictionary that does not exist. Key: " + key);

                // Exit the method early since the key does not exist.
                return;
            }

            // Remove the key-value pair from the dictionary.
            Remove(key);
        }
    }
}
