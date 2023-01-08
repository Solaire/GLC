namespace core_2
{

    /// <summary>
    /// Interface describing basic database data entry
    /// </summary>
    public interface IData
    {
        /// <summary>
        /// Primary key field
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Name field
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Flag determining if the entry is enabled
        /// </summary>
        public bool IsEnabled { get; }
    }
}
