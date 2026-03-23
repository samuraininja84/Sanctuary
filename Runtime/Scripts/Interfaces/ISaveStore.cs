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
        /// Invoked right before the data is saved to the memory.
        /// </summary>
        /// <param name="save">The current save controller.</param>
        void OnSave(SaveControllerBase save);

        /// <summary>
        /// Invoked when the data is loaded from the memory.
        /// </summary>
        /// <param name="save">The current save controller.</param>
        void OnLoad(SaveControllerBase save);
    }
}
