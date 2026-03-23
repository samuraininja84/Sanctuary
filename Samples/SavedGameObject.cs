using UnityEngine;
using Sanctuary;
using Sanctuary.Stores;
using Sanctuary.Attributes;

namespace Sanctaury.Samples
{
    /// <summary>
    /// An example implementation of <see cref="ISaveStore"/> that saves and loads the position, rotation, velocity, and active state of a GameObject.
    /// </summary>
    /// <remarks>
    /// Used to save and load the <see cref="GameObject.transform.position"/>, <see cref="GameObject.transform.rotation"/>, <see cref="Rigidbody.velocity"/>, and <see cref="GameObject.activeSelf"/> of a Target GameObject.
    /// </remarks>
    public class SavedGameObject : MonoBehaviour, ISaveStore
    {
        [SerializeField, ObjectLocation] private SaveLocation _location;

        [Header("Options")]
        [SerializeField] private SaveScope scope = SaveScope.Global;
        [SerializeField] private GameObject targetObject = null;
        [SerializeField] private bool resetVelocityOnLoad = true;

        private GameObjectData data = new();
        private Rigidbody rb;

        private Rigidbody RB => rb ??= targetObject.GetComponent<Rigidbody>();

        public void OnEnable() => SaveStoreRegistry.Register(this, SaveProvider.Global);

        public void OnDisable() => SaveStoreRegistry.Unregister(this);

        public void OnSave(SaveControllerBase save)
        {
            // Update the stored data from the target GameObject
            data.FromGameObject(targetObject, RB);

            // Save the active state of the GameObject
            save.Data.SetChunkName(_location, gameObject.name).Write(_location, data);
        }

        public void OnLoad(SaveControllerBase save)
        {
            // Try to read the active state from the save controller
            if (save.Data.TryRead(_location, data))
            {
                // Optionally, you could also set the name
                save.Data.SetChunkName(_location, gameObject.name);

                // Apply the loaded active state to the GameObject
                Apply(data);
            }
        }

        private void Apply(GameObjectData data)
        {
            // Set the position and rotation of the GameObject
            targetObject.transform.position = data.position;
            targetObject.transform.rotation = data.rotation;

            // If the GameObject has a rigidbody, set its velocity
            if (RB != null)            
            {
                // Set the velocity of the rigidbody
                RB.linearVelocity = resetVelocityOnLoad ? Vector3.zero : data.velocity;
                RB.angularVelocity = Vector3.zero;
            }

            // Set the active state of the GameObject
            targetObject.SetActive(data.isActive);
        }

        private class GameObjectData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 velocity;
            public bool isActive;

            public void FromGameObject(GameObject go, Rigidbody rb = null)
            {
                // Set the position, rotation, and active state from the GameObject
                position = go.transform.position;
                rotation = go.transform.rotation;
                isActive = go.activeSelf;
                
                // Optionally, you could also set the velocity if a rigidbody is provided
                if (rb != null) velocity = rb.linearVelocity;
            }
        }
    }
}
