using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.UI.Pages.Pie;

[Authorize(Roles = "SystemAdmin")]
public partial class DisplayModel : PageModel
{
    public PieModel Pie { get; set; }
    public string ImageURL { get; set; }

    private readonly ILogger<DisplayModel> _logger;
    private readonly IConfiguration _config;
    private readonly PieService _pieService;

    public DisplayModel(ILogger<DisplayModel> logger, IConfiguration config, PieService pieService)
    {
        this._logger = logger;
        this._config = config;
        this._pieService = pieService;
    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        Pie = await _pieService.GetPieById(id);
		ImageURL = "/images/" + (string.IsNullOrEmpty(Pie.ImageUrl) ? "NoImage.jpg" : "pie/" + Pie.ImageUrl);
		ViewData["Title"] = Pie.Name;
        return Page();
    });
}