namespace Sanctuary
{
    /// <summary>
    /// Specifies the type of save data used in the application.
    /// </summary>
    /// <remarks>
    /// This enumeration defines the different categories of save data, which determine how the data is stored and accessed. 
    /// Use the appropriate value based on the scope and persistence requirements of the save data: 
    /// <list type="bullet"> 
    /// <item> <description> <see cref="Temporary"/>: Save data that is temporary and not persisted across sessions.</description> </item> 
    /// <item> <description> <see cref="Scene"/>: Save data that is specific to a single save slot and scene context with indexed subdirectories for sorting.</description> </item>
    /// <item> <description> <see cref="Global"/>: Save data that is specific to a single save slot.</description> </item> 
    /// <item> <description> <see cref="Absolute"/>: Save data with absolute scope, unaffected by session, scene, or slot context.</description> </item>
    /// </list>
    /// </remarks>
    public enum SaveScope
    {
        /// <summary> 
        /// Save and load data local to the current session.
        /// </summary>
        /// <remarks>Recommended for temporary data that doesn't need to persist between sessions.</remarks>
        Temporary = 0,
        /// <summary>
        /// Save and load data specific to the current scene.
        /// </summary>
        /// <remarks>Recommended for scene-specific data like level progress and environment states.</remarks>
        Scene = 1,
        /// <summary>
        /// Save and load data specific to the current save slot.
        /// </summary>
        /// <remarks>Recommended for most gameplay data.</remarks>
        Global = 2,
        /// <summary>
        /// Save and load data with absolute scope, unaffected by session, scene, or slot context.
        /// </summary>
        /// <remarks>Recommended for use in debugging tools or global settings.</remarks>
        Absolute = 3
    }
}
