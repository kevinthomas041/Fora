using Fora.Extensions;
using Fora.Interfaces;
using Fora.Models;
using Fora.Settings;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Fora.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IncomeDataController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly EDGAR_Settings _settings;
        private readonly ILogger<IncomeDataController> _logger;
        private readonly IFundableAmountBuilder _fundableAmountBuilder;

        public IncomeDataController(HttpClient httpClient, EDGAR_Settings settings, IFundableAmountBuilder fundableAmountBuilder, ILogger<IncomeDataController> logger)
        {
            _httpClient = httpClient;
            _settings = settings;
            _fundableAmountBuilder = fundableAmountBuilder;
            _logger = logger;
        }

        //[HttpGet]
        //public async Task<IEnumerable<EdgarCompanyInfo>> GetSummariesAll()
        //{
        //    try
        //    {
        //        List<EdgarCompanyInfo> allCompanyInfo = new List<EdgarCompanyInfo>();

        //        for (int i = 0; i < _settings.CIK_Values.Length; i++)
        //        {
        //            var currentCIK = _settings.CIK_Values[i];
        //            var companyInfo = await GetCompanyInfoByCIK(currentCIK);

        //            if (companyInfo != null)
        //            {
        //                allCompanyInfo.Add(companyInfo);
        //            }
        //        }

        //        return allCompanyInfo;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        throw;
        //    }
        //}

        /// <summary>
        /// endpoint for retrieving a list of companies as well as the
        /// amount of funding they are eligible to receive.The request should optionally allow the user to
        /// supply a parameter that can be used to return only companies where their name starts with the
        /// specified letter. 
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSummaries/{letter}")]
        public async Task<IActionResult> GetSummariesByLetter(char letter)
        {
            if (!char.IsLetter(letter))
            {
                return BadRequest("value must be a single letter");
            }

            try
            {
                List<AmountInfo> ammountInfo = new List<AmountInfo>();

                for (int i = 0; i < _settings.CIK_Values.Length; i++)
                {
                    var currentCIK = _settings.CIK_Values[i];
                    var companyInfo = await GetCompanyInfoByCIK(currentCIK);

                    if (!string.IsNullOrWhiteSpace(companyInfo?.EntityName) && companyInfo.EntityName.ToUpper()[0] == char.ToUpper(letter))
                    {
                        ammountInfo.Add(_fundableAmountBuilder.Build(companyInfo));
                    }
                }
                
                var options = new JsonSerializerOptions { WriteIndented = true, };
                string jsonString = JsonSerializer.Serialize(ammountInfo, options);

                return Ok(ammountInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [NonAction]
        private async Task<EdgarCompanyInfo> GetCompanyInfoByCIK(int currentCIK)
        {
            try
            {
                var request = await _httpClient.GetAsync(currentCIK.ToCIKRoute());

                if (request.StatusCode == HttpStatusCode.OK)
                {
                    EdgarCompanyInfo? edgarCompanyInfo = await request.Content.ReadFromJsonAsync<EdgarCompanyInfo>();
                    return edgarCompanyInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return null;
        }
    }
}