using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ElasticSearch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class elasticSearch_Documents_SearchController : ControllerBase
    {
        private readonly ILogger<elasticSearch_Documents_SearchController> _logger;
        private readonly Services.IElasticService elasticService;

        #region Constructors
        public elasticSearch_Documents_SearchController(
            ILogger<elasticSearch_Documents_SearchController> logger,
            Services.IElasticService elasticService)
        {
            _logger = logger;
            this.elasticService = elasticService;
        }
        #endregion

        #region Search - Query With Aggrigation
        [HttpPost()]
        [Route("Search/Query/Aggrigation")]
        public async Task<IActionResult> Search_ByText_MultiColumn_BoostField(
           string indexName = "",
           int pageIndex = 1)
        {
            var fieldName = nameof(Domains.News.headline);

            string text = "party planning";

            var lst = await elasticService.SearchAggregationsAsync<Domains.News>(
                       indexName,
                       q => q.Match(m => m.Field(fieldName) // Specify the field to match
                                          .Query(text) // The query string
                                          .MinimumShouldMatch(2)), // Specify minimum should match
                       aggregations => aggregations.Add("agg_name",
                                                        aggregation => aggregation.Max(max => max.Field(x => x.date))),
                       pageIndex,
                       100);

            return Ok(lst);
        }
        #endregion

        #region Search - Filter Date Time
        [HttpGet()]
        [Route("Search/DateTime")]
        public async Task<IActionResult> Search_DateTime(string indexName = "", string dateStart = "2015_07_21", string dateEnd = "2015_07_22", int pageIndex = 1)
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

        #region Search - Filter By Text - Minimum Match
        /// <summary>
        /// Search Some Words in text + Minimum Should Match
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="fieldName"></param>
        /// <param name="text"></param>
        /// <param name="MinimumShouldMatch"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet()]
        [Route("Search/ByText/MinimumMatch")]
        public async Task<IActionResult> Search_ByText_MinimumMatch(
            string indexName = "",
            string fieldName = nameof(Domains.News.headline),
            string text = "Street Protests Hit Tehran, 2 Demonstrators Reported Killed In Western Iran",
            int MinimumShouldMatch = 2,
            int pageIndex = 1)
        {
            var lst = await elasticService.SearchAsync<Domains.News>(
                indexName,
                q => q.Match(m => m.Field(fieldName) // Specify the field to match
                                   .Query(text) // The query string
                                   .MinimumShouldMatch(MinimumShouldMatch) // Specify minimum should match
                ),
                pageIndex,
                100);

            return Ok(lst);
        }
        #endregion

        #region Search - Filter By Text - Exactly Text
        /// <summary>
        /// Search Text Exactly 
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="fieldName"></param>
        /// <param name="text"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet()]
        [Route("Search/ByText/TextExactly")]
        public async Task<IActionResult> Search_ByText_TextExactly(
            string indexName = "",
            string fieldName = nameof(Domains.News.headline),
            string text = "Street Protests Hit Tehran",
            int pageIndex = 1)
        {
            var lst = await elasticService.SearchAsync<Domains.News>(
                indexName,
                q => q.MatchPhrase(mp => mp.Field(fieldName) // Specify the field to match
                                           .Query(text) // The phrase to match
                ),
                pageIndex,
                100);

            return Ok(lst);
        }
        #endregion

        #region Search - Filter By Text - Multi Column
        [HttpPost()]
        [Route("Search/ByText/MultiColumn")]
        public async Task<IActionResult> Search_ByText_MultiColumn(
           string indexName = "",
           [FromBody] List<string> fieldsName = null!,
           string text = "Iran tehran",
           Elastic.Clients.Elasticsearch.QueryDsl.Operator operation = Elastic.Clients.Elasticsearch.QueryDsl.Operator.Or,
           int pageIndex = 1)
        {
            fieldsName ??= new List<string>
            {
                nameof(Domains.News.headline),
                nameof(Domains.News.category),
                nameof(Domains.News.short_description)+"^2"
            };

            var lst = await elasticService.SearchAsync<Domains.News>(
                indexName,
                q => q
                    .MultiMatch(mm => mm
                        .Query(text) // The query string  
                        .Fields(fieldsName.ToArray()) // Corrected field specification  
                        .Operator(operation) // Specify the operator as 'and'  
                    ),
                pageIndex,
                100);

            return Ok(lst);
        }
        #endregion

        #region Search - Filter By Text - Exactly Text - Boost Field
        [HttpPost()]
        [Route("Search/ByText/MultiColumn/BoostField")]
        public async Task<IActionResult> Search_ByText_MultiColumn_BoostField(
           string indexName = "",
           [FromBody] List<string> fieldsName = null!,
           string text = "party planning",
           Elastic.Clients.Elasticsearch.QueryDsl.TextQueryType textQueryType = Elastic.Clients.Elasticsearch.QueryDsl.TextQueryType.Phrase,
           int pageIndex = 1)
        {
            fieldsName ??= new List<string>
            {
                nameof(Domains.News.headline),
                nameof(Domains.News.category),
                nameof(Domains.News.short_description)+"^2"
            };

            var lst = await elasticService.SearchAsync<Domains.News>(
                indexName,
                q => q
                    .MultiMatch(mm => mm
                        .Query(text) // The query string  
                        .Fields(fieldsName.ToArray()) // Corrected field specification  
                        .Type(textQueryType) // Specify the type as phrase
                    ),
                pageIndex,
                100);

            return Ok(lst);
        }
        #endregion
    }
}
