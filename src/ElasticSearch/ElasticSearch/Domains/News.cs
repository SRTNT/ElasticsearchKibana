using Newtonsoft.Json;

namespace ElasticSearch.Domains
{
    public class News
    {
        public DateTime date { get; set; }
        public string? short_description { get; set; }

        [JsonProperty("@timestamp")]
        public DateTime timestamp { get; set; }
        public string? link { get; set; }
        public string? category { get; set; }
        public string? headline { get; set; }
        public string? authors { get; set; }
    }
}
