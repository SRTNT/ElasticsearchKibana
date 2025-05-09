namespace ElasticSearch.Domains
{
    public class ElasticsearchSetting
    {
        public string url { get; set; } = "";
        public string defaultIndex { get; set; } = "";
        public string userName { get; set; } = "";
        public string password { get; set; } = "";
    }
}
