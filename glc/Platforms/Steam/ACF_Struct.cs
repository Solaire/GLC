using System.Text;

namespace Steam
{
    internal class ACF_Struct
    {
        public Dictionary<string, ACF_Struct> SubACF { get; private set; }
        public Dictionary<string, string> SubItems { get; private set; }

        public ACF_Struct()
        {
            SubACF = new Dictionary<string, ACF_Struct>();
            SubItems = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return ToString(0);
        }

        private string ToString(int Depth)
        {
            StringBuilder SB = new();
            foreach(KeyValuePair<string, string> item in SubItems)
            {
                SB.Append('\t', Depth);
                SB.AppendFormat("\"{0}\"\t\t\"{1}\"\r\n", item.Key, item.Value);
            }
            foreach(KeyValuePair<string, ACF_Struct> item in SubACF)
            {
                SB.Append('\t', Depth);
                SB.AppendFormat("\"{0}\"\n", item.Key);
                SB.Append('\t', Depth);
                SB.AppendLine("{");
                SB.Append(item.Value.ToString(Depth + 1));
                SB.Append('\t', Depth);
                SB.AppendLine("}");
            }
            return SB.ToString();
        }
    }
}