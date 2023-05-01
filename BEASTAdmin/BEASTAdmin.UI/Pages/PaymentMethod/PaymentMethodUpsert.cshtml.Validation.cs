using BEASTAdmin.Core.Validator;

namespace BEASTAdmin.UI.Pages.PaymentMethod;

public partial class PaymentMethodUpsertModel
{
    private async Task<bool> ValidatePost()
    {
        var validationResult = await new PaymentMethodModelValidator().ValidateAsync(paymentMethod);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }
}