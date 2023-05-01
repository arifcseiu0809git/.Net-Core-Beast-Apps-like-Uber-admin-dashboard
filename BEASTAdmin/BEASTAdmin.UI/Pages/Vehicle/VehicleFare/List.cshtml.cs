using BEASTAdmin.Core.Infrastructure;
using BEASTAdmin.Core.Model;
using BEASTAdmin.Core.Model.Vehicle;
using BEASTAdmin.Service.Vehicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace BEASTAdmin.UI.Pages.Vehicle.VehicleFare
{
    [Authorize(Roles = "SystemAdmin")]
    public partial class ListModel : PageModel
    {
        public List<VehicleFareModel> vehicleFares { get; set; }
        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }

        private readonly ILogger<ListModel> _logger;
        private readonly VehicleFareService vehicleFareService;
        public ListModel(ILogger<ListModel> logger, VehicleFareService vehicleFareService)
        {
            _logger = logger;
            this.vehicleFareService = vehicleFareService;
        }

        public Task<IActionResult> OnGet() =>
        TryCatch(async () =>
        {
            var paginatedList = await vehicleFareService.GetVehicleFares(PageNumber);
            vehicleFares = paginatedList.Items;
            TotalRecords = paginatedList.TotalRecords;
            TotalPages = paginatedList.TotalPages;

            return Page();
        });

        public Task<IActionResult> OnPostDelete(string id) =>
        TryCatch(async () =>
        {
            LogModel logModel = new LogModel();
            logModel.UserName = User.Identity.Name;
            logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
            logModel.IP = Utility.GetIPAddress(Request);
            await vehicleFareService.DeleteVehicleFare(id, logModel);
            return RedirectToPage("/Vehicle/VehicleFare/List", new { c = "vehicleFare", p = "vehicleFareList" });
        });
        public Task<IActionResult> OnPostExport() =>
        TryCatch(async () =>
        {
            var exportFile = await vehicleFareService.Export();
            return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
        });
    }
}
