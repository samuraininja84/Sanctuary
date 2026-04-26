using System.Threading.Tasks;

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
        /// Invoked when the store is created for the first time. This happens only once during the lifetime of the save, and only if the store is registered before the first save.
        /// </summary>
        /// <param name="save">The save controller that provides context or state required for the load operation. Cannot be null.</param>
        virtual void OnCreate(SaveControllerBase save) { }

        /// <summary>
        /// Prepares the current object for serialization or saving by performing any necessary state updates.  
        /// </summary>
        /// <param name="save">The save controller that manages the save operation. Cannot be null.</param>
        virtual Task PrepareForSave(SaveControllerBase save) => Task.CompletedTask;

        /// <summary>
        /// Invoked right before the data is saved to the memory.
        /// </summary>
        /// <param name="save">The save controller that provides context or state required for the load operation. Cannot be null.</param>
        void OnSave(SaveControllerBase save);

        /// <summary>
        /// Performs additional processing after a save operation completes.
        /// </summary>
        /// <param name="save">The controller managing the save operation. Cannot be null.</param>
        virtual Task PostSave(SaveControllerBase save) => Task.CompletedTask;

        /// <summary>
        /// Prepares the current instance for loading by performing any necessary initialization using the specified save controller.
        /// </summary>
        /// <param name="save">The save controller that provides context or state required for the load operation. Cannot be null.</param>
        virtual Task PrepareForLoad(SaveControllerBase save) => Task.CompletedTask;

        /// <summary>
        /// Invoked when the data is loaded from the memory.
        /// </summary>
        /// <param name="save">The save controller that provides context or state required for the load operation. Cannot be null.</param>
        void OnLoad(SaveControllerBase save);

        /// <summary>
        /// Performs additional processing after a load operation completes.
        /// </summary>
        /// <param name="save">The save controller that provides context or state required for the load operation. Cannot be null.</param>
        virtual Task PostLoad(SaveControllerBase save) => Task.CompletedTask;
    }
}
