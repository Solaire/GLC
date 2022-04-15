namespace core
{
    /// <summary>
    /// Interface describing a typical data node found in database tables.
    /// <br/>
    /// Only the <see cref="IsEnabled"/> flag is mutable, but implementing objects can
    /// include their own mutators.
    /// </summary>
    public interface IDataNode
    {
        /// <summary>
        /// The primary key field.
        /// </summary>
        public int      PrimaryKey  { get; }

        /// <summary>
        /// The name field.
        /// </summary>
        public string   Name        { get; }

        /// <summary>
        /// The description field.
        /// </summary>
        public string   Description { get; }

        /// <summary>
        /// Flag controlling if the data object is enabled/active.
        /// </summary>
        public bool     IsEnabled   { get; set; }
    }

    /// <summary>
    /// Basic implementation of the <see cref="IDataNode"/> interface.
    /// <br/>
    /// The <see cref="PrimaryKey"/> field is readonly.
    /// </summary>
    public struct CDataNode : IDataNode
    {
        private readonly int id;
        private string  name;
        private string  description;
        private bool    isEnabled;

        public int PrimaryKey
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        public CDataNode(int id, string name, string description, bool isEnabled)
        {
            this.id          = id;
            this.name        = name;
            this.description = description;
            this.isEnabled   = isEnabled;
        }
    }
}
