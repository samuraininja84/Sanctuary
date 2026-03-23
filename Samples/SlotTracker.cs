using System.Collections.Generic;
using UnityEngine;
using Sanctuary.Stores;
using Sanctuary.Attributes;
using System.Threading.Tasks;

namespace Sanctuary.Samples
{
    public class SlotTracker : MonoBehaviour, ISaveStore
    {
        [SerializeField, ObjectLocation(true)] private SaveLocation _location;

        [Header("Runtime")]
        public SlotData data = SlotData.Default();
        public List<SlotData> allData = null;

        public void OnEnable() => SaveStoreRegistry.Register(this, SaveProvider.Global);

        public void OnDisable() => SaveStoreRegistry.Unregister(this);

        private void Update() => UpdateTimeSpent(Time.deltaTime);

        public void OnSave(SaveControllerBase save)
        {
            // Update completion before saving
            UpdateCompletion();

            // Save the data using the game object's name as the chunk name
            save.Data.SetChunkName(_location, gameObject.name).Write(_location, data);
        }

        public void OnLoad(SaveControllerBase save) => save.Data.SetChunkName(_location, gameObject.name).TryRead(_location, data);

        [ContextMenu("Load All")]
        public bool TryLoadAll() => TryLoadAll(SaveProvider.Global).Result;

        public async Task<bool> TryLoadAll(SaveControllerBase save) => await save.TryLoadAll(_location, allData);

        public async Task<List<SlotData>> TryGetAll() => await TryGetAll(SaveProvider.Global);

        public async Task<List<SlotData>> TryGetAll(SaveControllerBase save)
        {
            // Initialize the list to hold all loaded data
            var foundData = new List<SlotData>();

            // Try to load all data from the specified location. If successful, return the list; otherwise, return null.
            return await save.TryLoadAll(_location, foundData) ? foundData : null;
        }

        public void SetName(string name) => data.SetName(name);

        public void SetTimeSpent(float timeSpent) => data.SetTimeSpent(timeSpent);

        public void UpdateTimeSpent(float delta) => data.SetTimeSpent(data.timeSpent + delta);

        public void UpdateCompletion() => data.SetCompletion(CompletionExtensions.AsPercentage());

        public string GetName() => data.GetName();

        public string GetTimeStarted() => data.GetTimeStarted();

        public string GetTimeSpent() => data.GetTimeSpent();

        public float GetCompletion() => data.GetCompletion();

        public void ResetData() => data.Reset();
    }
}
