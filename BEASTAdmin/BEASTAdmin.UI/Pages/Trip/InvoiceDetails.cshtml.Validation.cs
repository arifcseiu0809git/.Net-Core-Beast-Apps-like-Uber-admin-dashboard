using BEASTAdmin.Core.Resources;
using BEASTAdmin.Core.Validator;

namespace BEASTAdmin.UI.Pages.Trip;

public partial class InvoiceDetailsModel
{

    private async Task<bool> ValidateModel()
    {
        var validationResult = await new TripModelValidator().ValidateAsync(Trip);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }

}