using System.Text.Json;
using url_shortener;

namespace url_shortener.Models
{
    public class URLMapping {
        public string url { get; set; }
        public string code { get; set; }

        [System.Text.Json.Serialization.JsonConstructor]
        public URLMapping(string url, string code)
        {
            this.url = url;
            this.code = code;
        }

        public URLMapping(string url)
        {
            this.url = url;
            this.code = Utils.Utils.GenerateCode();
        }

        public async static Task<List<URLMapping>> loadURLMappingFromDisk()
        {
            try
            {
                using var file = File.OpenRead(Path.Combine(Config.DataPath, "urlmapping.json"));
                var result = await JsonSerializer.DeserializeAsync<List<URLMapping>>(file);
                return result ?? new List<URLMapping>();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return new List<URLMapping>(); 
            }
        }

        public async static Task saveURLMappingToDisk(List<URLMapping> data)
        {
            try
            {
                using var file = File.OpenWrite(Path.Combine(Config.DataPath, "urlmapping.json"));
                await JsonSerializer.SerializeAsync(file, data);
                return;
            }
            catch (Exception e) { Console.WriteLine(e); return; }
        }
    }
}
