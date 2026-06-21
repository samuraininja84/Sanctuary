using System.Collections.Generic;
using UnityEngine;

namespace Sanctuary
{
    /// <summary>
    /// Represents a serializable list of items of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="SerializableList{T}"/> class extends the standard <see cref="List{T}"/> and implements <see cref="ISerializationCallbackReceiver"/> to support serialization in Unity. 
    /// During serialization, items are stored in a separate list to ensure compatibility with Unity's serialization system.
    /// This class is particularly useful for scenarios where lists need to be serialized in Unity, such as saving game data or configuring serialized objects.
    /// </remarks>
    /// <typeparam name="T">The type of items contained in the list.</typeparam>
    [System.Serializable]
    public class SerializableList<T> : List<T>, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The list of items to be serialized.
        /// </summary>
        [SerializeField] private List<T> items = new List<T>();

        /// <summary>
        /// Public accessor for the serialized items.
        /// </summary>
        public List<T> Items => items;

        public void OnBeforeSerialize()
        {
            // Clear existing items before serialization
            items.Clear();

            // Add all items to the serialized list
            foreach (var item in this) items.Add(item);
        }

        public void OnAfterDeserialize()
        {
            // Clear the current list
            this.Clear();

            // Add deserialized items back to the list
            foreach (var item in items) this.Add(item);
        }
    }
}
