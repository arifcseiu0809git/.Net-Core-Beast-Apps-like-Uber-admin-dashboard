using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using BEASTAPI.Endpoint.Resources;

namespace BEASTAPI.Endpoint.Controllers.V1;

public partial class TransactionDetailController
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

            if (returningFunction.Method.Name.Contains("GetTransactionDetails"))
                Messages = ExceptionMessages.Pie_List;

            if (returningFunction.Method.Name.Contains("GetTransactionDetailById"))
                Messages = ValidationMessages.Pie_NotFoundId;

            if (returningFunction.Method.Name.Contains("GetTransactionDetailByCategoryId"))
                Messages = ExceptionMessages.Pie_CategoryId;

            if (returningFunction.Method.Name.Contains("InsertTransactionDetail"))
                Messages = ExceptionMessages.Pie_Insert;

            if (returningFunction.Method.Name.Contains("UpdateTransactionDetail"))
                Messages = ExceptionMessages.Pie_Update;

            if (returningFunction.Method.Name.Contains("DeleteTransactionDetail"))
                Messages = ExceptionMessages.Pie_Delete;

            if (returningFunction.Method.Name.Contains("Export"))
                Messages = ExceptionMessages.Pie_List;

            return StatusCode(StatusCodes.Status500InternalServerError, Messages);
        }
        finally
        {
            // Do clean up code here, if needed.
        }
    }
}