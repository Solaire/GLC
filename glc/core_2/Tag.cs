using SqlDB;

namespace core_2
{
    public class CTag : IData
    {
        #region IData

        public int ID
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public bool IsEnabled
        {
            get;
            private set;
        }

        #endregion IData

        public string Description
        {
            get;
            private set;
        }

        private CTag() { }

        public static CTag CreateNew(int id, string name, bool isEnabled, string description)
        {
            return new CTag()
            {
                ID = id,
                Name = name,
                IsEnabled = isEnabled,
                Description = description
            };
        }

        /*
        internal CTag CreateFromDB(CSqlQry qry)
        {
            return new CTag()
            {
                ID = id,
                Name = name,
                IsEnabled = isEnabled,
                Description = description
            };
        }
        */
    }
}
