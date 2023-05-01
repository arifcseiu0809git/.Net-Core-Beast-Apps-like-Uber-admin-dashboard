using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using BEASTAPI.Infrastructure;
using System;
using System.Threading.Tasks;
using BEASTAPI.Endpoint.Resources;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Contract.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace BEASTAPI.Endpoint.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]

    public partial class CouponController : ControllerBase
    {
        private readonly ISecurityHelper _securityHelper;
        private readonly ILogger<PieController> _logger;
        private readonly IConfiguration _config;
        private readonly ICouponRepository _couponRepository;
        private readonly ICsvExporter _csvExporter;

        public CouponController(ISecurityHelper securityHelper, ILogger<PieController> logger, IConfiguration config, ICouponRepository couponRepository, ICsvExporter csvExporter)
        {
            this._securityHelper = securityHelper;
            this._logger = logger;
            this._config = config;
            this._couponRepository = couponRepository;
            this._csvExporter = csvExporter;
        }


        [HttpGet]
        public Task<IActionResult> GetCoupon(int pageNumber) =>
       TryCatch(async () =>
       {
           if (Convert.ToBoolean(_config["Hash:HashChecking"]))
           {
               if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                   return Unauthorized(ValidationMessages.InvalidHash);
           }

           if (pageNumber <= 0)
               return BadRequest(String.Format(ValidationMessages.Coupon_InvalidPageNumber, pageNumber));

           var result = await _couponRepository.GetCoupon(pageNumber);
           if (result == null)
               return NotFound(ValidationMessages.Coupon_NotFoundList);

           return Ok(result);
       });

        [HttpGet("{CouponId}"), AllowAnonymous]
        public Task<IActionResult> GetCouponById(string couponId) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), couponId))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (couponId == null)
                return BadRequest(String.Format(ValidationMessages.Coupon_InvalidId, couponId));

            var result = await _couponRepository.GetCouponById(couponId);
            if (result == null)
                return NotFound(String.Format(ValidationMessages.Coupon_NotFoundId, couponId));

            return Ok(result);
        });

        [HttpPost("InsertCoupon")]
        public Task<IActionResult> InsertCoupon([FromBody] Dictionary<string, object> PostData) =>
        TryCatch(async () =>
        {
            CouponModel coupon = JsonSerializer.Deserialize<CouponModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            coupon.Id = Guid.NewGuid().ToString();
            LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), coupon.UserId))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (coupon == null)
                return BadRequest(ValidationMessages.Coupon_Null);

            string insertedCouponId = await _couponRepository.InsertCoupon(coupon, logModel);
            return Created(nameof(GetCouponById), new { id = insertedCouponId });
        });

        [HttpPut("Update/{id}")]
        public Task<IActionResult> UpdateCoupon(string id, [FromBody] Dictionary<string, object> PostData) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            CouponModel coupon = JsonSerializer.Deserialize<CouponModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            LogModel logModel = JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (coupon == null)
                return BadRequest(ValidationMessages.Coupon_Null);

            if (id != coupon.Id)
                return BadRequest(ValidationMessages.Coupon_Mismatch);

            var CouponToUpdate = await _couponRepository.GetCouponById(id);
            if (CouponToUpdate == null)
                return NotFound(String.Format(ValidationMessages.Coupon_NotFoundId, id));

            await _couponRepository.UpdateCoupon(coupon, logModel);

            return NoContent(); // success
        });

        [HttpPut("Delete/{id}"), AllowAnonymous]
        public Task<IActionResult> DeleteCoupon(string id, [FromBody] LogModel logModel) =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            if (id == null)
                return BadRequest(String.Format(ValidationMessages.Coupon_InvalidId, id));

            var CouponToDelete = await _couponRepository.GetCouponById(id);
            if (CouponToDelete == null)
                return NotFound(String.Format(ValidationMessages.Coupon_NotFoundId, id));

            await _couponRepository.DeleteCoupon(id, logModel);

            return NoContent(); // success
        });

        [HttpGet("Export")]
        public Task<IActionResult> Export() =>
        TryCatch(async () =>
        {
            if (Convert.ToBoolean(_config["Hash:HashChecking"]))
            {
                if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                    return Unauthorized(ValidationMessages.InvalidHash);
            }

            var result = await _couponRepository.Export();
            if (result == null)
                return NotFound(ValidationMessages.Coupon_NotFoundList);

            return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
        });

    }
}
