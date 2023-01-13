namespace Steam
{
    internal static class Extension
    {
        public static int NextEndOf(this string str, char Open, char Close, int startIndex)
        {
            if(Open == Close)
                throw new Exception("\"Open\" and \"Close\" char are equivalent!");

            int OpenItem = 0;
            int CloseItem = 0;
            for(int i = startIndex; i < str.Length; i++)
            {
                if(str[i] == Open)
                {
                    OpenItem++;
                }
                if(str[i] == Close)
                {
                    CloseItem++;
                    if(CloseItem > OpenItem)
                        return i;
                }
            }
            throw new Exception("Not enough closing characters!");
        }
    }
}
