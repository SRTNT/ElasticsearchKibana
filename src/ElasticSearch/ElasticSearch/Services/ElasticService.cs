using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Transport;
using ElasticSearch.Domains;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ElasticSearch.Services
{
    public class ElasticService : IElasticService
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly ElasticsearchSetting _elasticsearchSetting;

        private string GetIndexName(string indexName)
        { return string.IsNullOrEmpty(indexName) ? _elasticsearchSetting.defaultIndex : indexName; }

        #region Constructors  
        public ElasticService(
            IOptions<ElasticsearchSetting> options)
        {
            _elasticsearchSetting = options.Value;

            var settings = new ElasticsearchClientSettings(new Uri(_elasticsearchSetting.url))
                .Authentication(new BasicAuthentication(_elasticsearchSetting.userName, _elasticsearchSetting.password))
                .DefaultIndex(_elasticsearchSetting.defaultIndex)
                .DefaultMappingFor<User>(m => m.IndexName(_elasticsearchSetting.defaultIndex))
                .EnableDebugMode() // Enable debug mode
                .PrettyJson(); // Optional: Makes JSON output more readable;

            _elasticClient = new ElasticsearchClient(settings);
        }
        #endregion

        #region Check Existed Index
        public async Task<bool> GetIndexExistsAsync(string indexName)
        {
            var response = await _elasticClient.Indices.ExistsAsync(GetIndexName(indexName));
            return response.Exists;
        }
        #endregion

        #region Create Index
        public async Task<bool> CreateIndexAsync(string indexName, string? mapping = null)
        {
            if (await GetIndexExistsAsync(indexName))
                return true;

            indexName = GetIndexName(indexName);

            Elastic.Clients.Elasticsearch.IndexManagement.CreateIndexResponse response;
            if (string.IsNullOrEmpty(mapping))
            {
                response = await _elasticClient.Indices.CreateAsync(indexName);
            }
            else
            {
                response = await _elasticClient.Indices.CreateAsync(indexName, c => c.Mappings(m =>
                {
                    var mappingJson = JsonSerializer.Deserialize<Dictionary<string, object>>(mapping);
                    foreach (var field in mappingJson)
                    {
                        if (field.Value is JsonElement element && element.TryGetProperty("type", out var typeProperty))
                        {
                            var fieldType = typeProperty.GetString();
                            switch (fieldType)
                            {
                                case "text":
                                    m.Properties(p => p.Text(field.Key));
                                    break;
                                case "keyword":
                                    m.Properties(p => p.Keyword(field.Key));
                                    break;
                                case "date":
                                    m.Properties(p => p.Date(field.Key));
                                    break;
                                case "integer":
                                    m.Properties(p => p.IntegerNumber(field.Key));
                                    break;
                                default:
                                    throw new NotSupportedException($"Field type '{fieldType}' is not supported.");
                            }
                        }
                    }
                }));

            }

            return response.IsValidResponse;
        }
        #endregion

        #region Delete Index
        public async Task<bool> DeleteIndexAsync(string indexName)
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.Indices.DeleteAsync(indexName);
            return response.IsValidResponse;
        }
        #endregion

        #region Add Update
        public async Task<bool> AddUpdateDocumentAsync<T>(string indexName, T document) where T : class
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.IndexAsync(document,
                                                           idx => idx.Index(indexName)
                                                                     .OpType(OpType.Index));

            return response.IsValidResponse;
        }
        #endregion

        #region Delete Documentation
        public async Task<bool> DeleteDocumentAsync(string indexName, string id)
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.DeleteAsync<object>(indexName, id);
            return response.IsValidResponse;
        }
        #endregion


        #region Get By ID
        public async Task<T> GetByIDAsync<T>(string indexName, string id) where T : class
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.GetAsync<T>(indexName, id);
            return response.Source!; // Use null-forgiving operator to ensure compatibility with the interface  
        }
        #endregion

        #region Get All
        public async Task<IEnumerable<T>> GetAllInPageAsync<T>(string indexName, int pageIndex = 1, int pageSize = 1000) where T : class
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.SearchAsync<T>(s => s.Indices(indexName)
                                                                     .From((pageIndex - 1) * pageSize)
                                                                     .Size(100)
                                                                     .Query(q => q.MatchAll())); // MatchAll query to fetch all records

            return response.IsValidResponse ? response.Documents.ToList() : new List<T>();
        }
        #endregion
    }
}
