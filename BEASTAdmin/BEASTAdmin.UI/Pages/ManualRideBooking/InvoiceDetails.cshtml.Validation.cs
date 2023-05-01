using BEASTAdmin.Core.Resources;
using BEASTAdmin.Core.Validator;

namespace BEASTAdmin.UI.Pages.ManualRideBooking;

public partial class InvoiceDetailsModel
{

    private async Task<bool> ValidateModel()
    {
        var validationResult = await new TripInitialModelValidator().ValidateAsync(TripInitial);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }

}