using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using BEASTAPI.Endpoint.Resources;

namespace BEASTAPI.Endpoint.Controllers.V1;

public partial class SavedAddressController
{
    private delegate Task<IActionResult> ReturningFunction();
    private string Messages = "";

    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try
        {
            return await returningFunction();
        }
        catch (Exception ex)
        {
            _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });

            if (returningFunction.Method.Name.Contains("GetSavedAddresses"))
                Messages = ExceptionMessages.Address_List;             

            if (returningFunction.Method.Name.Contains("GetSavedAddressById"))
                Messages = ExceptionMessages.Address_List;

            if (returningFunction.Method.Name.Contains("InsertSavedAddress"))
                Messages = ExceptionMessages.Address_List;

            if (returningFunction.Method.Name.Contains("UpdateSavedAddress"))
                Messages = ExceptionMessages.Address_List;

            if (returningFunction.Method.Name.Contains("DeleteSavedAddress"))
                Messages = ExceptionMessages.Address_List; 

            if (returningFunction.Method.Name.Contains("Export"))
                Messages = ExceptionMessages.Address_List;

            return StatusCode(StatusCodes.Status500InternalServerError, Messages);
        }
        finally
        {
            // Do clean up code here, if needed.
        }
    }
}