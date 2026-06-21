using Path = System.IO.Path;

namespace Sanctuary
{
    /// <summary>
    /// Represents profile data used for saving and accessing user-specific or application-specific data.
    /// </summary>
    /// <remarks>
    /// The <see cref="ProfileData"/> structure encapsulates information about a profile, including its scope, name, and an optional identifier. 
    /// It provides factory methods for creating instances with predefined scopes (<see cref="Absolute(string)"/>, <see cref="Global(string, int)"/>, <see cref="Scene(string, int)"/>, and <see cref="Temporary(string)"/>), as well as methods for retrieving and modifying its properties. 
    /// The scope determines the context in which the profile data is saved or accessed: 
    /// <list type="bullet"> 
    /// <item><description><see cref="SaveScope.Absolute"/>: Represents global data shared across all users or sessions.</description></item> 
    /// <item><description><see cref="SaveScope.Global"/>: Represents data specific to a particular profile, identified by a unique ID.</description></item> 
    /// <item><description><see cref="SaveScope.Scene"/>: Represents data specific to a particular scene, identified by a unique ID.</description></item>
    /// <item><description><see cref="SaveScope.Temporary"/>: Represents temporary data that is not persisted.</description></item> 
    /// </list>
    /// </remarks>
    [System.Serializable]
    public struct ProfileData
    {
        public SaveScope scope;
        public string fileName;
        public static int Id = 0;

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="ProfileData"/> with a Absolute save data scope.
        /// </summary>
        /// <param name="fileName">The name associated with the absolute profile data. Cannot be null or empty.</param>
        /// <returns>A <see cref="ProfileData"/> instance configured with a absolute save data scope and the specified name.</returns>
        public static ProfileData Absolute(string fileName) => new ProfileData(SaveScope.Absolute, fileName);

        /// <summary>
        /// Creates a new instance of <see cref="ProfileData"/> with the specified name and ID, using the <see cref="SaveScope.Global"/> scope.
        /// </summary>
        /// <param name="fileName">The name associated with the profile. Cannot be null or empty.</param>
        /// <param name="id">The unique identifier for the profile. Must be a non-negative integer.</param>
        /// <returns>A new <see cref="ProfileData"/> instance configured with the specified name, ID, and scope.</returns>
        public static ProfileData Global(string fileName) => new ProfileData(SaveScope.Global, fileName);

        /// <summary>
        /// Creates a new instance of <see cref="ProfileData"/> with the specified name and ID, using the <see cref="SaveScope.Scene"/> scope.
        /// </summary>
        /// <param name="fileName">The name associated with the profile. Cannot be null or empty.</param>
        /// <param name="id">The unique identifier for the profile. Must be a non-negative integer.</param>
        /// <returns>A new <see cref="ProfileData"/> instance configured with the specified name, ID, and scope.</returns>
        public static ProfileData Scene(string fileName) => new ProfileData(SaveScope.Scene, fileName);

        /// <summary>
        /// Creates a new instance of <see cref="ProfileData"/> with a temporary save data scope.
        /// </summary>
        /// <param name="fileName">The name associated with the profile data. Cannot be null or empty.</param>
        /// <returns>A <see cref="ProfileData"/> instance configured with a temporary save data scope and the specified name.</returns>
        public static ProfileData Temporary(string fileName) => new ProfileData(SaveScope.Temporary, fileName);

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileData"/> class with the specified scope, name, and an optional ID.
        /// </summary>
        /// <param name="scope">The scope of the profile data, which determines its storage or usage context.</param>
        /// <param name="fileName">The name of the profile data. This cannot be null or empty.</param>
        /// <param name="id">An optional identifier for the profile data. Defaults to -1, which typically indicates no specific ID, such as for global or temporary profiles.</param>
        public ProfileData(SaveScope scope, string fileName)
        {
            // Set the scope for the profile data, which determines how the data is saved and accessed.
            this.scope = scope;

            // Set the file name for the profile data.
            this.fileName = fileName;

            // Validate the scope to ensure the ID is appropriate for the given scope.
            ValidateScope(scope);
        }

        #endregion

        #region Set Methods

        /// <summary>
        /// Sets the scope for saving data.
        /// </summary>
        /// <param name="scope">The scope to be used for saving data. This determines the context or boundaries within which the data will be saved.</param>
        public void SetScope(SaveScope scope)
        {
            // Assign the provided scope to the instance variable.
            this.scope = scope;

            // Validate the scope to ensure it meets the requirements for the current ID.
            ValidateScope(scope);
        }

        /// <summary>
        /// Sets the name of the file to be used.
        /// </summary>
        /// <param name="fileName">The name of the file. This value cannot be null or empty.</param>
        public void SetFileName(string fileName) => this.fileName = fileName;

        /// <summary>
        /// Sets the identifier for the current instance.
        /// </summary>
        /// <param name="id">The identifier to assign. Must be a non-negative integer.</param>
        public void SetId(int id)
        {
            // Assign the provided ID to the instance variable.
            Id = id;

            // Validate the scope to ensure the ID is appropriate for the given scope.
            ValidateScope(scope);
        }

        /// <summary>
        /// Validates the specified <see cref="SaveScope"/> and ensures that the associated ID is set correctly.
        /// </summary>
        /// <remarks>
        /// This method adjusts the ID based on the provided <paramref name="scope"/>: 
        /// <list type="bullet"> 
        /// <item><description>For <see cref="SaveScope.Absolute"/> and <see cref="SaveScope.Temporary"/>, the ID is set to -1.</description></item> 
        /// <item><description>For <see cref="SaveScope.Global"/>, the ID must be a non-negative integer; otherwise, an exception is thrown.</description></item> 
        /// </list>
        /// </remarks>
        /// <param name="scope">The scope of the save data to validate. Must be one of the defined <see cref="SaveScope"/> values.</param>
        /// <exception cref="System.ArgumentException">Thrown if the <paramref name="scope"/> is <see cref="SaveScope.Global"/> and the ID is a negative value.</exception>
        private void ValidateScope(SaveScope scope)
        {
            // Validate the ID based on the scope.
            switch (scope)
            {
                // Clamp ID for global scope.
                case SaveScope.Global:
                    if (Id < 0) Id = 0;
                    break;
                // Clamp ID for scenes.
                case SaveScope.Scene:
                    if (Id < 0) Id = 0;
                    break;
            }
        }

        #endregion

        #region Get Methods

        /// <summary>
        /// Gets the current <see cref="SaveScope"/> associated with this instance.
        /// </summary>
        /// <returns>The <see cref="SaveScope"/> representing the current scope of saved data.</returns>
        public SaveScope GetScope() => scope;

        /// <summary>
        /// Gets the name of the file associated with the current instance.
        /// </summary>
        /// <returns>The name of the file as a <see cref="string"/>. If no file name is set, returns an empty string.</returns>
        public string GetFileName() => fileName;

        /// <summary>
        /// Retrieves the unique identifier associated with the current instance.
        /// </summary>
        /// <returns>The unique identifier as an <see cref="int"/>.</returns>
        public int GetId() => Id;

        /// <summary>
        /// Determines whether the current profile scope is set to absolute.
        /// </summary>
        /// <returns><see langword="true"/> if the current profile scope is <see cref="SaveScope.Absolute"/>; otherwise, <see langword="false"/>.</returns>
        public bool IsAbsolute() => GetScope() == SaveScope.Absolute;

        /// <summary>
        /// Determines whether the current profile scope is set to global.
        /// </summary>
        /// <returns><see langword="true"/> if the current profile scope is <see cref="SaveScope.Global"/>; otherwise, <see langword="false"/>.</returns>
        public bool IsGlobal() => GetScope() == SaveScope.Global;

        /// <summary>
        /// Determines whether the profile is scene-specific.
        /// </summary>
        /// <returns><see langword="true"/> if the profile is <see cref="SaveScope.Scene"/>; otherwise, <see langword="false"/>.</returns>
        public bool IsScene() => GetScope() == SaveScope.Scene;

        /// <summary>
        /// Determines whether the profile is temporary.
        /// </summary>
        /// <returns><see langword="true"/> if the profile is <see cref="SaveScope.Temporary"/>; otherwise, <see langword="false"/>.</returns>
        public bool IsTemporary() => GetScope() == SaveScope.Temporary;

        /// <summary>
        /// Constructs a folder path based on the specified profile scope, identifier, and base folder path.
        /// </summary>
        /// <remarks>
        /// The method appends a scope-specific subdirectory to the provided <paramref name="folderPath"/>: 
        /// <list type="bullet"> 
        /// <item><description>For <see cref="ProfileScope.Global"/>, "Slots" is appended, followed by the <paramref name="id"/> if it is non-negative.</description></item>
        /// <item><description>For <see cref="ProfileScope.Scene"/>, "Slots" is appended, followed by the <paramref name="id"/> if it is non-negative.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="folderPath">The base folder path to which the scope-specific subdirectory will be appended.</param>
        /// <returns>
        /// A string representing the constructed folder path. 
        /// The path includes a scope-specific subdirectory ("Scene" or "Global") and, if applicable, 
        /// the identifier as a subdirectory for <see cref="ProfileScope.Global"/> or <see cref="ProfileScope.Scene"/>.
        /// </returns>
        public readonly string GetScopedPath(string folderPath)
        {
            // Based on the scope, adjust the folder path accordingly
            switch (scope)
            {
                // If the scope is Absolute, append Absolute to the folder path as a subdirectory
                case SaveScope.Absolute:
                    folderPath = Path.Combine(folderPath, "Absolute");
                    break;
                // If the scope is Global, append the an ID to the folder path id as a subdirectory
                case SaveScope.Global:
                    folderPath = Path.Combine(folderPath, $"{Id}");
                    break;
                // If the scope is Scene, append the an ID to the folder path id as a subdirectory
                case SaveScope.Scene:
                    folderPath = Path.Combine(folderPath, $"{Id}");
                    break;
            }

            // Return the final folder path based on the scope
            return folderPath;
        }

        #endregion

        /// <summary>
        /// Returns a string representation of the profile data, including the file name, ID, and scope.
        /// </summary>
        /// <returns>A string containing the profile data in the format: "Profile Data: {fileName} (ID: {id}, Scope: {scope})".</returns>
        public override string ToString() => $"Profile Data: {fileName} (ID: {Id}, Scope: {scope})";
    }
}