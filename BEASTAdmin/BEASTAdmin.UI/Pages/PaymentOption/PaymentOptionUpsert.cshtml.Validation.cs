using BEASTAdmin.Core.Validator;

namespace BEASTAdmin.UI.Pages.PaymentOption;

public partial class PaymentOptionUpsertModel
{
    private async Task<bool> ValidatePost()
    {
        var validationResult = await new PaymentOptionModelValidator().ValidateAsync(paymentOption);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }
}