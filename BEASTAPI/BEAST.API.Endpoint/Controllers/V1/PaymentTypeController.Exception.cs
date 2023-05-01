using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using BEASTAPI.Endpoint.Resources;

namespace BEASTAPI.Endpoint.Controllers.V1;

public partial class PaymentTypeController
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

            if (returningFunction.Method.Name.Contains("GetPaymentTypes"))
                Messages = ExceptionMessages.Address_List;             

            if (returningFunction.Method.Name.Contains("GetPaymentTypeById"))
                Messages = ExceptionMessages.Address_List;

            if (returningFunction.Method.Name.Contains("GetPaymentTypeByName"))
                Messages = ExceptionMessages.Address_List;

            if (returningFunction.Method.Name.Contains("InsertPaymentType"))
                Messages = ExceptionMessages.Address_List;

            if (returningFunction.Method.Name.Contains("UpdatePaymentType"))
                Messages = ExceptionMessages.Address_List;

            if (returningFunction.Method.Name.Contains("DeletePaymentType"))
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