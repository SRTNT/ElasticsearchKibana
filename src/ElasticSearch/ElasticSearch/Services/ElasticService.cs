using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Clients.Elasticsearch.QueryDsl;
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
                .DefaultMappingFor<News>(m => m.IndexName(_elasticsearchSetting.defaultIndex))
                .EnableDebugMode() // Enable debug mode
                .PrettyJson(); // Optional: Makes JSON output more readable;

            _elasticClient = new ElasticsearchClient(settings);
        }
        #endregion

        #region Check Existed Index
        public async Task<bool> GetIndexExistsAsync(string indexName)
        {
            var response = await _elasticClient.Indices.ExistsAsync(GetIndexName(indexName));
            return response.IsValidResponse ? response.Exists : false;
        }
        #endregion

        #region Create Index
        public async Task<bool> CreateIndexAsync<T>(string indexName, Action<Elastic.Clients.Elasticsearch.Mapping.TypeMappingDescriptor<T>>? mapping = null)
        {
            if (await GetIndexExistsAsync(indexName))
                return true;

            indexName = GetIndexName(indexName);

            Elastic.Clients.Elasticsearch.IndexManagement.CreateIndexResponse response;
            if (mapping is null)
            {
                response = await _elasticClient.Indices.CreateAsync
                    (index: indexName,
                     action: c => c.Settings(s => s.NumberOfShards(1) // شاردها بخش‌های ایندکس هستند که می‌توانند به صورت موازی پردازش شوند.
                                                   .NumberOfReplicas(1))); // نسخه‌های پشتیبان برای افزایش دسترسی و تحمل خطا استفاده می‌شوند.
            }
            else
            {
                response = await _elasticClient.Indices.CreateAsync
                    (index: indexName,
                     action: c => c.Settings(s => s.NumberOfShards(1) // شاردها بخش‌های ایندکس هستند که می‌توانند به صورت موازی پردازش شوند.
                                                   .NumberOfReplicas(1)) // نسخه‌های پشتیبان برای افزایش دسترسی و تحمل خطا استفاده می‌شوند.
                                   .Mappings(mapping)
                    );

                /*.Mappings(m =>
                  {
                      var mappingJson = JsonSerializer.Deserialize<Dictionary<string, object>>(mapping)!;
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
                  })*/
            }

            return response.IsValidResponse && response.IsSuccess();
        }
        #endregion

        #region Delete Index
        public async Task<bool> DeleteIndexAsync(string indexName)
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.Indices.DeleteAsync(indexName);
            return response.IsValidResponse && response.IsSuccess();
        }
        #endregion

        #region Add Update
        public async Task<bool> AddUpdateDocumentAsync<T>(string indexName, T data) where T : class
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.IndexAsync(data,
                                                           idx => idx.Index(indexName)
                                                                     .OpType(OpType.Index));

            return response.IsValidResponse && response.IsSuccess();
        }
        #endregion

        #region Delete Documentation
        public async Task<bool> DeleteDocumentAsync(string indexName, string id)
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.DeleteAsync<object>(indexName, id);
            return response.IsValidResponse && response.IsSuccess();
        }
        #endregion

        #region Get By ID
        public async Task<T?> GetByIDAsync<T>(string indexName, string id) where T : class
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.GetAsync<T>(indexName, id);
            return response.IsValidResponse && response.IsSuccess() ? response.Source : null; // Use null-forgiving operator to ensure compatibility with the interface  
        }
        #endregion

        #region Get All
        public async Task<IEnumerable<T>> GetAllInPageAsync<T>(string indexName, int pageIndex = 1, int pageSize = 1000) where T : class
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.SearchAsync<T>(s => s.Indices(indexName)
                                                                     .From((pageIndex - 1) * pageSize)
                                                                     .Size(pageSize)
                                                                     .Query(q => q.MatchAll())); // MatchAll query to fetch all records

            return response.IsValidResponse && response.IsSuccess()
                ? response.Documents.ToList()
                : new List<T>();
        }
        #endregion

        #region Search
        public async Task<IEnumerable<T>> SearchAsync<T>(string indexName, Action<QueryDescriptor<T>> query, int pageIndex = 1, int pageSize = 10)
        {
            indexName = GetIndexName(indexName);

            var response = await _elasticClient.SearchAsync<T>(s => s.Indices(indexName)
                                                                     .Query(query)
                                                                     .From((pageIndex - 1) * pageSize)
                                                                     .Size(pageSize));

            return response.IsValidResponse && response.IsSuccess()
                ? response.Documents.ToList()
                : new List<T>();
        }
        #endregion

        #region Get Count
        public async Task<long> GetCount(string indexName)
        {
            indexName = GetIndexName(indexName);
            var response = await _elasticClient.CountAsync<object>(c => c.Indices(indexName));
            return response.IsValidResponse && response.IsSuccess() ? response.Count : 0;
        }
        #endregion
    }
}
