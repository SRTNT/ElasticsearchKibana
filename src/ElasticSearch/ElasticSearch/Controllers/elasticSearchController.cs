using ElasticSearch.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class elasticSearchController : ControllerBase
    {
        private readonly ILogger<elasticSearchController> _logger;
        private readonly IElasticService elasticService;

        public elasticSearchController(ILogger<elasticSearchController> logger, IElasticService elasticService)
        {
            _logger = logger;
            this.elasticService = elasticService;
        }

        #region Index
        [HttpGet()]
        public async Task<IActionResult> ExistedIndex(string indexName = "")
        {
            return Ok(await elasticService.GetIndexExistsAsync(indexName));
        }

        [HttpPost()]
        public async Task<IActionResult> AddIndex(string indexName = "")
        {
            await elasticService.CreateIndexAsync(indexName);
            return Ok(await elasticService.GetIndexExistsAsync(indexName));
        }

        [HttpDelete()]
        public async Task<IActionResult> DeleteIndex(string indexName = "")
        {
            await elasticService.DeleteIndexAsync(indexName);
            return Ok(!await elasticService.GetIndexExistsAsync(indexName));
        }

        [HttpGet()]
        [Route("GetCountRecordOfIndex")]
        public async Task<IActionResult> GetCountRecordOfIndex(string indexName = "")
        {
            return Ok(await elasticService.GetCount(indexName));
        }
        #endregion

        #region Documents
        [HttpPost()]
        [Route("AddUpdateDocument")]
        public async Task<IActionResult> AddUpdaetDocument(Domains.User user, string indexName = "")
        {
            await elasticService.AddUpdateDocumentAsync(indexName, user);
            return Ok();
        }

        [HttpDelete()]
        [Route("DeleteDocuments/{id}")]
        public async Task<IActionResult> DeleteDocuments(int id, string indexName = "")
        {
            await elasticService.DeleteDocumentAsync(indexName, id.ToString());
            return Ok();
        }

        [HttpGet()]
        [Route("GetByID/{id}")]
        public async Task<IActionResult> GetByID(int id, string indexName = "")
        {
            return Ok(await elasticService.GetByIDAsync<Domains.User>(indexName, id.ToString()));
        }

        [HttpGet()]
        [Route("GetAllInPage")]
        public async Task<IActionResult> GetAllInPage(string indexName = "", int pageIndex = 1)
        {
            return Ok(await elasticService.GetAllInPageAsync<Domains.User>(indexName, pageIndex, 100));
        }
        #endregion
    }
}
