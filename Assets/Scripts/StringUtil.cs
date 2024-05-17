public class StringUtil
{
    public static string[] ListTrimSplit(string s)
    {
        return s.Trim(new char[] { '[', ']' }).Split(",");
        
    }
}