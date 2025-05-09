namespace ElasticSearch.Services
{
    public interface IElasticService
    {
        Task<bool> GetIndexExistsAsync(string indexName);
        Task<bool> CreateIndexAsync<T>(string indexName, Action<Elastic.Clients.Elasticsearch.Mapping.TypeMappingDescriptor<T>>? mapping = null);
        Task<bool> DeleteIndexAsync(string indexName);

        Task<bool> AddUpdateDocumentAsync<T>(string indexName, T document) where T : class;
        Task<bool> DeleteDocumentAsync(string indexName, string id);

        Task<T?> GetByIDAsync<T>(string indexName, string id) where T : class;
        Task<IEnumerable<T>> GetAllInPageAsync<T>(string indexName, int pageIndex = 1, int pageSize = 1000) where T : class;
        Task<IEnumerable<T>> SearchAsync<T>(string indexName, Action<Elastic.Clients.Elasticsearch.QueryDsl.QueryDescriptor<T>> query, int pageIndex = 1, int pageSize = 10);

        Task<long> GetCount(string indexName);
    }
}
