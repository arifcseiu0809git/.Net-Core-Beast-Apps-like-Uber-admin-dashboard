using BEASTAdmin.Core.Validator;

namespace BEASTAdmin.UI.Pages.SavedAddress;

public partial class UpsertModel
{
    private async Task<bool> ValidatePost()
    {
        var validationResult = await new SavedAddressModelValidator().ValidateAsync(savedAddress);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }
}