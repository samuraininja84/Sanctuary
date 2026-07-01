namespace Sanctuary.Stores
{
    /// <summary>
    /// An interface with callbacks for saving and loading.
    /// </summary>
    /// <remarks>
    /// Objects implementing this interface should be registered with <see cref="SaveStoreRegistry"/>.
    /// It can be done manually, by adding the <see cref="SaveStoreDispatcher"/> component to the same game object, or by saving directly via <see cref="SaveStoreRegistry"/> methods.
    /// </remarks>
    public interface ISaveStore 
    {
        /// <summary>
        /// The Unity object that is associated with this store. This can be used to identify the store in the Unity Editor or to access its properties and methods.
        /// </summary>
        public UnityEngine.Object Source { get; }

        /// <summary>
        /// Invoked when the store is created for the first time. This happens only once during the lifetime of the save, and only if the store is registered before the first save.
        /// </summary>
        /// <param name="save">The save controller that provides context or state required for the load operation. Cannot be null.</param>
        virtual void OnCreate(SaveControllerBase save) { }

        /// <summary>
        /// Invoked right before the data is saved to the memory.
        /// </summary>
        /// <param name="save">The save controller that provides context or state required for the load operation. Cannot be null.</param>
        void OnSave(SaveControllerBase save);

        /// <summary>
        /// Invoked when the data is loaded from the memory.
        /// </summary>
        /// <param name="save">The save controller that provides context or state required for the load operation. Cannot be null.</param>
        void OnLoad(SaveControllerBase save);
    }
}
