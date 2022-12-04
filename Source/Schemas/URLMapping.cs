using System.Runtime.InteropServices;

namespace url_shortener.Models
{
    public class URLMapping {
        public string url { get; set; }
        public string code { get; set; }
        public DateTime created_at { get; set; }

        public int? access_count {get;}

        public URLMapping(string url, string code, DateTime created_at, [Optional] int? access_count)
        {
            this.url = url;
            this.code = code;
            this.created_at = created_at;
            this.access_count = access_count;
        }
    }

    public class URLMappingInput
    {
        public string code { get; set; }
        public string url { get; set; }

        public URLMappingInput(string? url, string? code)
        {
            this.code = code ?? Utils.Utils.GenerateCode();
            this.url = url ?? "";
        }
    }
}
