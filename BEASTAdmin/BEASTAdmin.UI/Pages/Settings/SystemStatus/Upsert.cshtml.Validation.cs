using BEASTAdmin.Core.Validator.Common;

namespace BEASTAdmin.UI.Pages.Settings.SystemStatus;

public partial class UpsertModel
{
    private async Task<bool> ValidatePost()
    {
        var validationResult = await new SystemStatusModelValidator().ValidateAsync(SystemStatus);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }
}