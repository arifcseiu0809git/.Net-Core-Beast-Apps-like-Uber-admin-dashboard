using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Contract.Persistence;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Map;
using BEASTAPI.Core.ViewModel;
using BEASTAPI.Endpoint.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace BEASTAPI.Endpoint.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]
    public partial class AdminEarningController : ControllerBase
    {
        private readonly ISecurityHelper _securityHelper;
        private readonly ILogger<AdminEarningController> _logger;
        private readonly IConfiguration _config;
        private readonly IAdminEarningRepository _adminEarningRepository;
        private readonly ICsvExporter _csvExporter;

        public AdminEarningController(
            ISecurityHelper securityHelper,
            ILogger<AdminEarningController> logger, 
            IConfiguration config, 
            IAdminEarningRepository adminEarningRepository,
            ICsvExporter csvExporter)
        {
            this._securityHelper = securityHelper;
            this._logger = logger;
            this._config = config;
            this._adminEarningRepository = adminEarningRepository;
            this._csvExporter = csvExporter;
        }

        [HttpGet]
        public Task<IActionResult> GetEarning(int pageNumber) =>
        TryCatch(async () =>
        {
            #region Validation
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (pageNumber <= 0) return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

            var result = await _adminEarningRepository.GetEarning(pageNumber);
            if (result == null)
                return NotFound(ValidationMessages.Pie_NotFoundList);
            #endregion

            return Ok(result);
        });

        [HttpGet("GetEarningByDriverId")]
        public Task<IActionResult> GetEarningByDriverId(string driverId,int pageNumber) =>
        TryCatch(async () =>
        {
            #region Validation
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (pageNumber <= 0) return BadRequest(String.Format(ValidationMessages.Pie_InvalidPageNumber, pageNumber));

            var result = await _adminEarningRepository.GetEarningByDriverId(driverId, pageNumber);
            if (result == null)
                return NotFound(ValidationMessages.Pie_NotFoundList);
            #endregion

            return Ok(result);
        });

        [HttpGet("Export")]
        public Task<IActionResult> Export([FromQuery]string driverId) =>
        TryCatch(async () =>
        {
            #region Validation
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            var result = await _adminEarningRepository.Export(driverId);
            if (result == null)
                return NotFound(ValidationMessages.Category_NotFoundList);
            #endregion

            return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
        });

        [HttpPost]
        public Task<IActionResult> AddDriverCommissions([FromBody] Dictionary<string, object> PostData) =>
        TryCatch(async () =>
        {
            AdminEarningInsertViewModel adminEarningInsertViewModel = JsonSerializer.Deserialize<AdminEarningInsertViewModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            adminEarningInsertViewModel.Id = new Guid().ToString();
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), adminEarningInsertViewModel.DriverId))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (adminEarningInsertViewModel == null)
                return BadRequest(ValidationMessages.Pie_Null);

            await _adminEarningRepository.InsertDriverCommissionsByDateRange(adminEarningInsertViewModel, logModel);
            return Ok("Successful!");
        });
    

        [HttpGet("GetDueCommission")]
        public Task<IActionResult> GetDueCommission([FromQuery]string driverId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) =>
        TryCatch(async () =>
        {
            #region Validation
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), driverId.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (String.IsNullOrEmpty(driverId) || fromDate == null || toDate == null) return BadRequest("Invalid request!");

            var result = await _adminEarningRepository.GetDueCommissionByDriverId(driverId, fromDate, toDate);
            if (result == null)
                return NotFound(ValidationMessages.Pie_NotFoundList);
            #endregion

            return Ok(result);
        });

    }
}
