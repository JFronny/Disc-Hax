namespace Shared
{
    public static class NullBoolToString
    {
        public static string GetString(this bool? self)
        {
            try
            {
                return self.ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}