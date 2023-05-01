using BEASTAdmin.Core.Validator;

namespace BEASTAdmin.UI.Pages.PaymentType;

public partial class PaymentTypeUpsertModel
{
    private async Task<bool> ValidatePost()
    {
        var isValidModel = true;
        var isDuplicateExists = true;
        var validationResult = await new PaymentTypeModelValidator().ValidateAsync(paymentType);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            isValidModel = false;
        }

        isDuplicateExists = await _paymentTypeService.CheckIfDuplicateExists(paymentType.Id, paymentType.Name);

        return isValidModel && !isDuplicateExists;
    }
}