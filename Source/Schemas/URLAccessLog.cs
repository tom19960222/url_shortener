using System.Runtime.InteropServices;

namespace url_shortener.Models
{
    public record class URLAccessLog(string code, [Optional] DateTime? accessed_at);
}
