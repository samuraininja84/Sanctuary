using System.IO;
using UnityEngine;

namespace Sanctuary.Adapters
{
    /// <summary>
    /// ScriptableObject implementation of <see cref="ISanctuaryConfiguration"/> that roots Sanctuary's save
    /// directory at <c>Application.persistentDataPath</c>. Drop one of these into a
    /// <c>SanctuaryApplicationLoader</c> to override the core package's default configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "Sanctuary Configuration", menuName = "Sanctuary/Sanctuary Configuration")]
    public sealed class SanctuaryConfigurationSO : ScriptableObject, ISanctuaryConfiguration
    {
        [Tooltip("Subdirectory under Application.persistentDataPath where save files live.")]
        [SerializeField] private string m_FolderName = "Saves";

        [Tooltip("Schema version stamped onto new save envelopes. Bump when your save format changes; pair with an ISaveMigrationStep for the upgrade.")]
        [SerializeField] private int m_CurrentSchemaVersion = 1;

        public string RootPath => Path.Combine(Application.persistentDataPath, m_FolderName);
        public int CurrentSchemaVersion => m_CurrentSchemaVersion;
    }
}
