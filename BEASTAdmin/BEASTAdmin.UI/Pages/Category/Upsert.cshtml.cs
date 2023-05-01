using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Resources;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Infrastructure;

namespace BEASTAdmin.UI.Pages.Category;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public CategoryModel Category { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly CategoryService _categoryService;

    public UpsertModel(ILogger<UpsertModel> logger, CategoryService categoryService)
    {
        this._logger = logger;
        this._categoryService = categoryService;
    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            Category = new CategoryModel();
        }
        else
        {
            Category = await _categoryService.GetCategoryById(id);
        }

        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (await ValidatePost() == false) return Page();

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = "192.168.1.1";

        if (string.IsNullOrEmpty(Category.Id))
        {
            Category.CreatedBy = User.Identity.Name;
            Category.IsActive = true;
            Category.IsDeleted = false;
            await _categoryService.InsertCategory(Category, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            Category.ModifiedBy = User.Identity.Name;
            Category.ModifiedDate = DateTime.Now;
            await _categoryService.UpdateCategory(Category.Id, Category, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        return Page();
    });
}