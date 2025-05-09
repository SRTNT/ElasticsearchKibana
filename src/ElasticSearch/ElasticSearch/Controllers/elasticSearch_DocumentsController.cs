using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticSearch.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

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
        public async Task<IActionResult> AddUpdaetDocument(Domains.News user, string indexName = "")
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
            return Ok(await elasticService.GetByIDAsync<Domains.News>(indexName, id.ToString()));
        }
        #endregion

        #region Get All In Page
        [HttpGet()]
        [Route("GetAllInPage")]
        public async Task<IActionResult> GetAllInPage(string indexName = "", int pageIndex = 1)
        {
            return Ok(await elasticService.GetAllInPageAsync<Domains.News>(indexName, pageIndex, 100));
        }
        #endregion

        #region Search
        [HttpGet()]
        [Route("Search/FilterDateTime")]
        public async Task<IActionResult> Search_FilterDateTime(string indexName = "", string dateStart = "2015_07_21", string dateEnd = "2015_07_22", int pageIndex = 1)
        {
            var lst = await elasticService.SearchAsync<Domains.News>(
                indexName,
                q => q.Range(r => r.Date(d => d.Field("date") // DateRange Filter
                                               .Gte(dateStart.Replace("_", "-"))
                                               .Lt(dateEnd.Replace("_", "-")))),
                pageIndex,
                100);

            if (lst.Any(q => q.date != new DateTime(2015, 07, 21)))
            { return StatusCode(500, "Internal server error: Filter has error"); }

            return Ok(lst);
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
