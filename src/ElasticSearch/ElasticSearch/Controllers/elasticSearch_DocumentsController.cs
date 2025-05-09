using ElasticSearch.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class elasticSearch_DocumentsController : ControllerBase
    {
        private readonly ILogger<elasticSearch_DocumentsController> _logger;
        private readonly IElasticService elasticService;

        #region Constructors
        public elasticSearch_DocumentsController(
            ILogger<elasticSearch_DocumentsController> logger,
            IElasticService elasticService)
        {
            _logger = logger;
            this.elasticService = elasticService;
        }
        #endregion

        #region Add Update Document
        [HttpPost()]
        [Route("AddUpdateDocument")]
        public async Task<IActionResult> AddUpdaetDocument(Domains.User user, string indexName = "")
        {
            await elasticService.AddUpdateDocumentAsync(indexName, user);
            return Ok();
        }
        #endregion

        #region Delete Documents
        [HttpDelete()]
        [Route("DeleteDocuments/{id}")]
        public async Task<IActionResult> DeleteDocuments(int id, string indexName = "")
        {
            await elasticService.DeleteDocumentAsync(indexName, id.ToString());
            return Ok();
        }
        #endregion

        #region Get By ID
        [HttpGet()]
        [Route("GetByID/{id}")]
        public async Task<IActionResult> GetByID(int id, string indexName = "")
        {
            return Ok(await elasticService.GetByIDAsync<Domains.User>(indexName, id.ToString()));
        }
        #endregion

        #region Get All In Page
        [HttpGet()]
        [Route("GetAllInPage")]
        public async Task<IActionResult> GetAllInPage(string indexName = "", int pageIndex = 1)
        {
            return Ok(await elasticService.GetAllInPageAsync<Domains.User>(indexName, pageIndex, 100));
        }
        #endregion

        #region Get Count Record Of Index
        [HttpGet()]
        [Route("GetCountRecordOfIndex")]
        public async Task<IActionResult> GetCountRecordOfIndex(string indexName = "")
        {
            return Ok(await elasticService.GetCount(indexName));
        }
        #endregion
    }
}
