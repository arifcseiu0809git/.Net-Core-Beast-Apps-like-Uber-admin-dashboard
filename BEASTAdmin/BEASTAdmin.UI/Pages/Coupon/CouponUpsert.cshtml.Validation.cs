using BEASTAdmin.Core.Validator;

namespace BEASTAdmin.UI.Pages.Coupon;

public partial class CouponUpsertModel
{
    private async Task<bool> ValidatePost()
    {
        var validationResult = await new CouponModelValidator().ValidateAsync(Coupon);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }
}