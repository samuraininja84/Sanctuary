using UnityEngine;
using Sanctuary.Extensions;

namespace Sanctuary
{
    /// <summary>
    /// An abstract base class for bootstrapping a SaveProvider in Unity.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(SaveProvider))]
    public abstract class Bootstrapper : MonoBehaviour
    {
        /// <summary>
        /// A reference to the SaveProvider instance managed by this Bootstrapper.
        /// </summary>
        protected SaveProvider container;

        /// <summary>
        /// The SaveProvider instance managed by this Bootstrapper.
        /// </summary>
        internal SaveProvider Container => container.OrNull() ?? (container = GetComponent<SaveProvider>());

        /// <summary>
        /// A flag indicating whether this Bootstrapper has already performed its bootstrap process.
        /// </summary>
        protected bool hasBeenBootstrapped;

        /// <summary>
        /// Bootstraps the SaveProvider on Awake.
        /// </summary>
        private void Awake() => BootstrapOnDemand();

        /// <summary>
        /// Bootstraps the SaveProvider if it hasn't been bootstrapped yet.
        /// </summary>
        public void BootstrapOnDemand()
        {
            // Avoid double bootstrap
            if (hasBeenBootstrapped) return;

            // Mark as bootstrapped
            hasBeenBootstrapped = true;

            // Setup the container
            Bootstrap();
        }

        /// <summary>
        /// Logic to bootstrap the SaveProvider. Implement in derived classes.
        /// </summary>
        protected abstract void Bootstrap();
    }
}
