namespace Client.Utility
{
    public static class StringExtensions
    {
        public static bool ContainsAtStart(string str, string con)
        {
            if (con.Length > str.Length) return false;
            for (int i = 0; i < con.Length; i++)
            {
                if (str[i] != con[i]) return false;
            }
            return true;
        }
    }
}
