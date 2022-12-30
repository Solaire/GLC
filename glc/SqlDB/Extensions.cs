using System.Text;

namespace SqlDB
{
    public static class CExtensions
    {
        /// <summary>
        /// StringBuilder extension method
        /// Returns the index of the start of the contents in a StringBuilder
        /// </summary>
        /// <param name="value">The string to find</param>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="ignoreCase">if set totrue it will ignore case</param>
        /// <returns></returns>
        public static int IndexOf(this StringBuilder sb, string value, int startIndex, bool ignoreCase)
        {
            int index;
            int length = value.Length;
            int maxSearchLength = (sb.Length - length) + 1;

            if(ignoreCase)
            {
                for(int i = startIndex; i < maxSearchLength; ++i)
                {
                    if(Char.ToLower(sb[i]) == Char.ToLower(value[0]))
                    {
                        index = 1;
                        while((index < length) && (Char.ToLower(sb[i + index]) == Char.ToLower(value[index])))
                        {
                            ++index;
                        }
                        if(index == length)
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }

            for(int i = startIndex; i < maxSearchLength; ++i)
            {
                if(sb[i] == value[0])
                {
                    index = 1;
                    while((index < length) && (sb[i + index] == value[index]))
                    {
                        ++index;
                    }

                    if(index == length)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
