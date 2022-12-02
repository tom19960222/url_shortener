namespace url_shortener.Utils;

public class Utils
{
    private static Random random = new Random();

    public static string GenerateCode()
    {
        const int length = 6;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
