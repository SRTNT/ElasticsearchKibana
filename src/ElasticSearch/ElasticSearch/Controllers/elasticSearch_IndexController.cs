using ElasticSearch.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class elasticSearch_IndexController : ControllerBase
    {
        private readonly ILogger<elasticSearch_IndexController> _logger;
        private readonly IElasticService elasticService;

        #region Constructors
        public elasticSearch_IndexController(
            ILogger<elasticSearch_IndexController> logger,
            IElasticService elasticService)
        {
            _logger = logger;
            this.elasticService = elasticService;
        }
        #endregion

        #region Existed Index
        [HttpGet()]
        public async Task<IActionResult> ExistedIndex(string indexName = "")
        {
            return Ok(await elasticService.GetIndexExistsAsync(indexName));
        }
        #endregion

        #region Add Index User
        [HttpPost()]
        public async Task<IActionResult> AddIndexUser(string indexName = "")
        {
            //await elasticService.CreateIndexAsync<Domains.News>
            //        (indexName: indexName,
            //         mapping: i => i.Properties(p => p.Text(q => q.firstName)
            //                                          .Text(q => q.lastName)
            //                                          .IntegerNumber(q => q.id))
            //        );

            await elasticService.CreateIndexAsync<Domains.News>
                    (indexName: indexName, null);

            return Ok(await elasticService.GetIndexExistsAsync(indexName));
        }
        #endregion

        #region Delete Index
        [HttpDelete()]
        public async Task<IActionResult> DeleteIndex(string indexName = "")
        {
            await elasticService.DeleteIndexAsync(indexName);
            return Ok(!await elasticService.GetIndexExistsAsync(indexName));
        }
        #endregion
    }
}
