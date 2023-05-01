using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using BEASTAPI.Endpoint.Resources;

namespace BEASTAPI.Endpoint.Controllers.V1.Common;

public partial class DocumentTypeController
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

            if (returningFunction.Method.Name.Contains("GetDocumentTypes"))
                Messages = ExceptionMessages.Pie_List;

            if (returningFunction.Method.Name.Contains("GetDocumentTypeById"))
                Messages = ValidationMessages.Pie_NotFoundId;

			if (returningFunction.Method.Name.Contains("InsertDocumentType"))
                Messages = ExceptionMessages.Pie_Insert;

            if (returningFunction.Method.Name.Contains("UpdateDocumentType"))
                Messages = ExceptionMessages.Pie_Update;

            if (returningFunction.Method.Name.Contains("DeleteDocumentType"))
                Messages = ExceptionMessages.Pie_Delete;

            return StatusCode(StatusCodes.Status500InternalServerError, Messages);
        }
        finally
        {
            // Do clean up code here, if needed.
        }
    }
}