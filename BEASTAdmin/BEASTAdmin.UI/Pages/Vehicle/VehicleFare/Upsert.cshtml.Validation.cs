using BEASTAdmin.Core.Resources;
using BEASTAdmin.Core.Validator.Vehicle;
using FluentValidation;

namespace BEASTAdmin.UI.Pages.Vehicle.VehicleFare;

public partial class UpsertModel
{
    private async Task<bool> ValidatePost()
    {
        bool isValid = true;

        isValid = await ValidateModel();

        return isValid;
    }

    private async Task<bool> ValidateModel()
    {
        var validationResult = await new VehicleFareValidator().ValidateAsync(vehicleFare);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }
}