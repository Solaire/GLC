namespace Steam
{
    /// <summary>
    /// Steam acf File Reader
    /// https://stackoverflow.com/a/42876399/6754996
    /// </<summary>
    internal class SteamWrapper
    {
        public string FileLocation { get; private set; }

        public SteamWrapper(string FileLocation)
        {
            if(File.Exists(FileLocation))
                this.FileLocation = FileLocation;
            else
                throw new FileNotFoundException("Error", FileLocation);
        }

        public bool CheckIntegrity()
        {
            string Content = File.ReadAllText(FileLocation);
            int quote = Content.Count(x => x == '"');
            int braceleft = Content.Count(x => x == '{');
            int braceright = Content.Count(x => x == '}');

            return ((braceleft == braceright) && (quote % 2 == 0));
        }

        public ACF_Struct ACFFileToStruct()
        {
            return ACFFileToStruct(File.ReadAllText(FileLocation));
        }

        private ACF_Struct ACFFileToStruct(string RegionToReadIn)
        {
            ACF_Struct ACF = new();
            int LengthOfRegion = RegionToReadIn.Length;
            int CurrentPos = 0;
            while(LengthOfRegion > CurrentPos)
            {
                int FirstItemStart = RegionToReadIn.IndexOf('"', CurrentPos);
                if(FirstItemStart == -1)
                    break;
                int FirstItemEnd = RegionToReadIn.IndexOf('"', FirstItemStart + 1);
                CurrentPos = FirstItemEnd + 1;
                string FirstItem = "";
                if(FirstItemEnd - FirstItemStart - 1 >= 0)
                    FirstItem = RegionToReadIn.Substring(FirstItemStart + 1, FirstItemEnd - FirstItemStart - 1);

                int SecondItemStartQuote = RegionToReadIn.IndexOf('"', CurrentPos);
                int SecondItemStartBraceleft = RegionToReadIn.IndexOf('{', CurrentPos);
                if(SecondItemStartBraceleft == -1 || SecondItemStartQuote < SecondItemStartBraceleft)
                {
                    int SecondItemEndQuote = RegionToReadIn.IndexOf('"', SecondItemStartQuote + 1);
                    string SecondItem = "";
                    if(SecondItemEndQuote - SecondItemStartQuote - 1 >= 0)
                        SecondItem = RegionToReadIn.Substring(SecondItemStartQuote + 1, SecondItemEndQuote - SecondItemStartQuote - 1);
                    CurrentPos = SecondItemEndQuote + 1;
                    if(!ACF.SubItems.ContainsKey(FirstItem))
                        ACF.SubItems.Add(FirstItem, SecondItem);
                    else
                        break;
                }
                else
                {
                    int SecondItemEndBraceright = RegionToReadIn.NextEndOf('{', '}', SecondItemStartBraceleft + 1);
                    ACF_Struct ACFS = null;
                    if(SecondItemEndBraceright - SecondItemStartBraceleft - 1 >= 0)
                        ACFS = ACFFileToStruct(RegionToReadIn.Substring(SecondItemStartBraceleft + 1, SecondItemEndBraceright - SecondItemStartBraceleft - 1));
                    CurrentPos = SecondItemEndBraceright + 1;
                    if(!ACF.SubACF.ContainsKey(FirstItem))
                        ACF.SubACF.Add(FirstItem, ACFS);
                }
            }

            return ACF;
        }
    }
}
