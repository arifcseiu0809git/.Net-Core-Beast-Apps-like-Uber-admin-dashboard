using BEASTAPI.Endpoint.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BEASTAPI.Endpoint.Controllers.V1
{
    public partial class AddressController
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
                
                if (returningFunction.Method.Name.Contains("GetAddress"))
                    Messages = ExceptionMessages.Address_List;

                if (returningFunction.Method.Name.Contains("GetAddressById"))
                    Messages = ValidationMessages.Address_NotFoundId;                 

                if (returningFunction.Method.Name.Contains("InsertAddress"))
                    Messages = ExceptionMessages.Address_Insert;

                if (returningFunction.Method.Name.Contains("UpdateAddress"))
                    Messages = ExceptionMessages.Address_Update;

                if (returningFunction.Method.Name.Contains("DeleteAddress"))
                    Messages = ExceptionMessages.Address_Delete;

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
}
