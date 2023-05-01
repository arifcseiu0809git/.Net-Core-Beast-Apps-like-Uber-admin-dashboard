using BEASTAdmin.Core.Resources;
using BEASTAdmin.Core.Validator.Vehicle;

namespace BEASTAdmin.UI.Pages.Vehicle.XDriverVehicle;

public partial class UpsertModel
{
    private async Task<bool> ValidatePost()
    {
        bool IsValid = true;

        IsValid = await ValidateModel();
       // if (IsValid) IsValid = ValidateImage();

        return IsValid;
    }

    private async Task<bool> ValidateModel()
    {
        var validationResult = await new XDriverVehicleValidator().ValidateAsync(xDriverVehicle);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }
}