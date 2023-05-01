using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BEASTAdmin.Service;
using Microsoft.AspNetCore.Authorization;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.UI.Pages.Driver;

[Authorize(Roles = "SystemAdmin")]
public partial class DisplayModel : PageModel
{
    public DriverModel Driver { get; set; }
    public List<TripModel> TripModels { get; set; }
    public int RideHistoryPageNumber { get; set; } = 1;
    public int TotalRideRecords { get; set; }
    public int TotalRidePages { get; set; }
    public int CancelTrips { get; set; }
    public int CompleteTrips { get; set; }
    public decimal Revenues { get; set; }
    public string ImageURL { get; set; }

    private readonly ILogger<DisplayModel> _logger;
    private readonly IConfiguration _config;
    private readonly DriverModelService _driverModelService;
    private readonly TripService _tripService;

    public DisplayModel(ILogger<DisplayModel> logger, IConfiguration config, DriverModelService driverModelService, TripService tripService)
    {
        this._logger = logger;
        this._config = config;
        this._driverModelService = driverModelService;
        _tripService = tripService;
    }

    public Task<IActionResult> OnGet(string id) =>
    TryCatch(async () =>
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound($"Unable to load driver with ID '{id}'.");
        }
        else
        {
            Driver = await _driverModelService.GetDriverModelById(id);
            await PopulatePageElements();
        }
        return Page();
    });

    private async Task PopulatePageElements()
    {
        await OnRideListPageClick(RideHistoryPageNumber);
        //var paginatedList = await _tripService.GetTripsByDriverId(Driver.Id, RideHistoryPageNumber);

        //TripModels = paginatedList.Items;
        //TotalRideRecords = paginatedList.TotalRecords;
        //TotalRidePages = paginatedList.TotalPages;
        //CancelTrips = TripModels.Where(x => x.StatusName == "Cancel").Count();
        //CompleteTrips = TripModels.Where(x => x.StatusName == "Success").Count();
        //Revenues = TripModels.Where(x => x.StatusName == "Success").Sum(x => x.Total);
    }

    protected async Task OnRideListPageClick(int ridePageNo)
    {
        var paginatedList = await _tripService.GetTripsByDriverId(Driver.Id, ridePageNo);

        TripModels = paginatedList.Items;
        TotalRideRecords = paginatedList.TotalRecords;
        TotalRidePages = paginatedList.TotalPages;
        CancelTrips = TripModels.Where(x => x.StatusName == "Cancel").Count();
        CompleteTrips = TripModels.Where(x => x.StatusName == "Success").Count();
        Revenues = TripModels.Where(x => x.StatusName == "Success").Sum(x => x.Total);
    }
}