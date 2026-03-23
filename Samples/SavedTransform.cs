using UnityEngine;
using Sanctuary;
using Sanctuary.Stores;
using Sanctuary.Attributes;

namespace Sanctaury.Samples
{
    /// <summary>
    /// An example implementation of <see cref="ISaveStore"/>
    /// </summary>
    /// <remarks>
    /// Used to save and load the <see cref="Transform.position"/>
    /// </remarks>
    public class SavedTransform : MonoBehaviour, ISaveStore
    {
        [SerializeField, ObjectLocation] private SaveLocation _location;

        [Header("Options")]
        [SerializeField] private SaveScope scope = SaveScope.Global;
        [SerializeField] private bool resetVelocityOnLoad = true;
        private TransformData data = new();

        public void OnEnable() => SaveStoreRegistry.Register(this, SaveProvider.ByScope(scope));

        public void OnDisable() => SaveStoreRegistry.Unregister(this);

        public void OnSave(SaveControllerBase save)
        {
            // Update the stored data
            data.position = transform.position;
            data.rotation = transform.rotation;

            // Save the data to the save controller
            save.Data.SetChunkName(_location, gameObject.name).Write(_location, data);
        }

        public void OnLoad(SaveControllerBase save)
        {
            // Try to read the data from the save controller
            if (save.Data.TryRead(_location, data))
            {
                // Apply the loaded data to the transform
                transform.position = data.position;
                transform.rotation = data.rotation;

                // Optionally reset velocity
                if (resetVelocityOnLoad) ResetVelocity();

                // Optionally, you could also set the name
                save.Data.SetChunkName(_location, gameObject.name);
            }
        }

        private void ResetVelocity()
        {
            // Get the rigidbody, if any, and reset its velocity
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Reset linear and angular velocity
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private class TransformData
        {
            public Vector3 position;
            public Quaternion rotation;
        }
    }
}
