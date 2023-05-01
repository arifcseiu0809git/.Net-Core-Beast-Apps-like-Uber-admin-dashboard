using BEASTAPI.Endpoint.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BEASTAPI.Endpoint.Controllers.V1
{
    public partial class CouponController
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

                if (returningFunction.Method.Name.Contains("GetCoupon"))
                    Messages = ExceptionMessages.Coupon_List;

                if (returningFunction.Method.Name.Contains("GetCouponById"))
                    Messages = ValidationMessages.Coupon_NotFoundId;

                if (returningFunction.Method.Name.Contains("InsertCoupon"))
                    Messages = ExceptionMessages.Coupon_Insert;

                if (returningFunction.Method.Name.Contains("UpdateCoupon"))
                    Messages = ExceptionMessages.Coupon_Update;

                if (returningFunction.Method.Name.Contains("DeleteCoupon"))
                    Messages = ExceptionMessages.Coupon_Delete;

                if (returningFunction.Method.Name.Contains("Export"))
                    Messages = ExceptionMessages.Coupon_List;


                return StatusCode(StatusCodes.Status500InternalServerError, Messages);
            }
            finally
            {
                // Do clean up code here, if needed.
            }
        }

    }
}
