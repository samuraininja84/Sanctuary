namespace Sanctuary 
{
    public readonly struct LoadResult
    {
        private readonly LoadStatus m_status;
        private readonly ISaveData m_data;

        /// <summary>
        /// Gets the result of the load operation, indicating whether it was successful or failed.
        /// </summary>
        public LoadStatus Status => m_status;

        /// <summary>
        /// Indicates whether the load operation was successful and the data is not null.
        /// </summary>
        public bool IsSuccess => m_status == LoadStatus.Success && m_data != null;

        /// <summary>
        /// Gets the loaded save data if the load operation was successful; otherwise, returns <see langword="null"/>.
        /// </summary>
        public ISaveData Data => m_data ?? null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadResult"/> struct with the specified result and save data.
        /// </summary>
        /// <param name="result">Indicates the result of the load operation.</param>
        /// <param name="data">The save data associated with the load operation.</param>
        private LoadResult(LoadStatus result, ISaveData data)
        {
            this.m_status = result;
            this.m_data = data;
        }

        /// <summary>
        /// Creates a new instance of <see cref="LoadResult"/> representing a successful load operation with the provided save data.
        /// </summary>
        /// <param name="data">The save data that was successfully loaded.</param>
        /// <returns>A <see cref="LoadResult"/> instance with <see cref="m_status"/> set to <see cref="LoadStatus.Success"/> and <see cref="Data"/> set to the provided save data.</returns>
        public static LoadResult Success(ISaveData data) => new(LoadStatus.Success, data);

        /// <summary>
        /// Creates a new instance of <see cref="LoadResult"/> representing a failed load operation.
        /// </summary>
        /// <returns>A <see cref="LoadResult"/> instance with <see cref="m_status"/> set to <see cref="LoadStatus.Failure"/> and <see cref="Data"/> set to <see langword="null"/>.</returns>
        public static LoadResult Failure() => new(LoadStatus.Failure, null);

        /// <summary>
        /// Defines the possible results of a load operation, indicating whether it was successful or failed.
        /// </summary>
        public enum LoadStatus
        {
            Success,
            Failure
        }
    }
}