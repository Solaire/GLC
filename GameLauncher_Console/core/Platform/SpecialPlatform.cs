using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core.Platform
{
    /// <summary>
    /// Enum describing any platforms with special properties. Negative numbers
    /// are used to distinguish them from database platforms.
    /// </summary>
    public enum SpecialPlatformID
    {
        cSearch     = -1,
        cFavourites = -2,
    }

    public class CSearchPlatform : CBasicPlatform
    {
        public CSearchPlatform()
            : base((int)SpecialPlatformID.cSearch, "Search", "", "", true)
        {

        }
    }

    public class CFavouritePlatform : CBasicPlatform
    {
        public CFavouritePlatform()
            : base((int)SpecialPlatformID.cFavourites, "Favourites", "", "", true)
        {

        }
    }
}
