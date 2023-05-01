using Microsoft.AspNetCore.Mvc;

namespace BEASTAdmin.UI.Pages.ManualRideBooking;

public partial class ManualRideBookingListModel
{
    private delegate Task<IActionResult> ReturningFunction();
    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try
        {
            return await returningFunction();
        }
        catch (Exception ex)
        {
            _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
            TempData["ErrorMessage"] = ex.Message;
            TempData["StackTrace"] = ex.StackTrace;
            return RedirectToPage("/Error");
        }
    }
}