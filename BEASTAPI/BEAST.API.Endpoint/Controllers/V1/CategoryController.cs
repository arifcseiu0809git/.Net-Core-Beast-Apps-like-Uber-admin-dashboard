using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BEASTAPI.Core.Model;
using System;
using System.Threading.Tasks;
using BEASTAPI.Endpoint.Resources;
using System.Collections.Generic;
using System.Text.Json;
using BEASTAPI.Core.Contract.Infrastructure;
using BEASTAPI.Core.Contract.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using BEASTAPI.Core.Constant;

namespace BEASTAPI.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class CategoryController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<CategoryController> _logger;
    private readonly IConfiguration _config;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICsvExporter _csvExporter;

    public CategoryController(ISecurityHelper securityHelper, ILogger<CategoryController> logger, IConfiguration config, ICategoryRepository categoryRepository, ICsvExporter csvExporter)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._categoryRepository = categoryRepository;
        this._csvExporter = csvExporter;
    }

    [HttpGet]
    public Task<IActionResult> GetCategories(int pageNumber) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (pageNumber < 0)
            return BadRequest(String.Format(ValidationMessages.Category_InvalidPageNumber, pageNumber));
        #endregion

        var result = await _categoryRepository.GetCategories(pageNumber);
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(result);
    });

    [HttpGet("GetDistinctCategories")]
    public Task<IActionResult> GetDistinctCategories() =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }
        #endregion

        var result = await _categoryRepository.GetDistinctCategories();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(result);
    });

    [HttpGet("{id}")]
    public Task<IActionResult> GetCategoryById(string id) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (id == "")
            return BadRequest(String.Format(ValidationMessages.Category_InvalidId, id));
        #endregion

        var result = await _categoryRepository.GetCategoryById(id);
        if (result == null)
            return NotFound(String.Format(ValidationMessages.Category_NotFoundId, id));

        return Ok(result);
    });

    [HttpGet("GetCategoriesWithPies")]
    public Task<IActionResult> GetCategoriesWithPies() =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }
        #endregion

        var result = await _categoryRepository.GetCategoriesWithPies();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundGetCategoriesWithPies);

        return Ok(result);
    });

    [HttpGet("Export")]
    public Task<IActionResult> Export() =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }
        #endregion

        var result = await _categoryRepository.Export();
        if (result == null)
            return NotFound(ValidationMessages.Category_NotFoundList);

        return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
    });

    [HttpPost("InsertCategory")]
    public Task<IActionResult> InsertCategory([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        CategoryModel category = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CategoryModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        category.Id = Guid.NewGuid().ToString();
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), category.Name))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (category == null) return BadRequest(ValidationMessages.Category_Null);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

        var existingCategory = await _categoryRepository.GetCategoryByName(category.Name);
        if (existingCategory != null)
            return BadRequest(String.Format(ValidationMessages.Category_Duplicate, category.Name));
        #endregion

        string insertedCategoryId = await _categoryRepository.InsertCategory(category, logModel);
        return Created(nameof(GetCategoryById), new { id = insertedCategoryId });
    });

    [HttpPut("Update/{categoryId}")]
    public Task<IActionResult> UpdateCategory(string categoryId, [FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        CategoryModel category = PostData["Data"] == null ? null : JsonSerializer.Deserialize<CategoryModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), categoryId.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (categoryId == "") return BadRequest(String.Format(ValidationMessages.Category_InvalidId, categoryId));
        if (category == null) return BadRequest(ValidationMessages.Category_Null);
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
        if (categoryId != category.Id) return BadRequest(ValidationMessages.Category_Mismatch);

        var categoryToUpdate = await _categoryRepository.GetCategoryById(categoryId);
        if (categoryToUpdate == null)
            return NotFound(String.Format(ValidationMessages.Category_NotFoundId, categoryId));
        #endregion

        await _categoryRepository.UpdateCategory(category, logModel);
        return NoContent(); // success
    });

    [HttpPut("Delete/{id}")]
    public Task<IActionResult> DeleteCategory(string id, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        #region Validation
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized(ValidationMessages.InvalidHash);
        }

        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (id == "") return BadRequest(String.Format(ValidationMessages.Category_InvalidId, id));
        if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

        var categoryToDelete = await _categoryRepository.GetCategoryById(id);
        if (categoryToDelete == null)
            return NotFound(String.Format(ValidationMessages.Category_NotFoundId, id));
        #endregion

        await _categoryRepository.DeleteCategory(id, logModel);
        return NoContent();
    });
}