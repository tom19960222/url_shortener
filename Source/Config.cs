namespace url_shortener
{
    public class Config
    {
        public static string DataPath
        {
            get
            {
                return Environment.GetEnvironmentVariable("DATA_PATH") ?? string.Empty;
            }
        }
    }
}
