using System.Threading;
using UnityEngine;

namespace Sanctuary.Adapters
{
    /// <summary>
    /// UnityApplicationLoaderBase that brings up a Fork application container. Optionally accepts
    /// a <see cref="SanctuaryConfigurationSO"/>; if none is assigned, the core package's default
    /// <see cref="DefaultSanctuaryConfiguration"/> is used (which writes to a relative <c>"Saves"</c>
    /// directory — useful for tests, but Unity consumers should supply an SO so saves land under
    /// <c>Application.persistentDataPath</c>).
    /// </summary>
    [CreateAssetMenu(fileName = "SanctuaryApplicationLoader", menuName = "Sanctuary/Application Loader")]
    public sealed class SanctuaryApplicationLoader : UnityApplicationLoaderBase
    {
        [Tooltip("Optional SanctuaryConfigurationSO asset. Leave empty to use the core package's default DefaultSanctuaryConfiguration.")]
        [SerializeField] private SanctuaryConfigurationSO m_Configuration;

        //private ApplicationContainer m_Application;

        public override Awaitable LoadAsync(CancellationToken cancellationToken)
        {
            //var builder = new ApplicationBuilder();
            //var collection = builder.UseForkPackage();

            //if (m_Configuration != null)
            //{
            //    collection.WithFactory<ISanctuaryConfiguration>(() => m_Configuration);
            //}

            //m_Application = builder.Build();
            return AwaitableUtility.CompletedTask;
        }

        public override Awaitable UnloadAsync()
        {
            //m_Application?.Dispose();
            //m_Application = null;
            return AwaitableUtility.CompletedTask;
        }
    }
}
