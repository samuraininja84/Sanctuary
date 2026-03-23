using UnityEngine;
using Sanctuary.Attributes;
using Sanctuary.Stores;

namespace Sanctuary.Samples
{
    public abstract class SerializableScriptableObject: ScriptableObject, ISaveStore
    {
        [SerializeField, ObjectLocation] private SaveLocation _location;

        [Header("Scope Options")]
        [SerializeField] private SaveScope scope = SaveScope.Global;

        public void Initialize() => SaveStoreRegistry.Register(this, SaveProvider.ByScope(scope));

        public void Dispose() => SaveStoreRegistry.Unregister(this);

        public virtual void OnSave(SaveControllerBase save) => save.Data.SetChunkName(_location, name).Write(_location, this);

        public virtual void OnLoad(SaveControllerBase save) => save.Data.SetChunkName(_location, name).TryRead(_location, this);
    }
}
