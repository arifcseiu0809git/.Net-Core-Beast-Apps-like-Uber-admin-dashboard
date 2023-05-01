using BEASTAdmin.Core.Resources;
using BEASTAdmin.Core.Validator;

namespace BEASTAdmin.UI.Pages.Passenger;

public partial class UpsertModel
{
    private async Task<bool> ValidatePost()
    {
        bool IsValid = true;

        IsValid = await ValidateModel();
        if (IsValid) IsValid = ValidateImage();

        return IsValid;
    }

    private async Task<bool> ValidateModel()
    {
        var validationResult = await new PassengerModelValidator().ValidateAsync(Passenger);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }
    private bool ValidateImage()
    {
        if (PassengerImage != null)
        {
            string[] supportedImageTypes = _config["SiteSettings:SupportedImageTypes"].Split(',');
            if (!supportedImageTypes.Contains(Path.GetExtension(PassengerImage.FileName).Substring(1)))
            {
                ErrorMessage = ValidationMessages.Pie_FileExtension;
                return false;
            }

            if ((PassengerImage.Length / 1024) > Convert.ToInt32(_config["SiteSettings:MaxImageSize"]))
            {
                ErrorMessage = ValidationMessages.Pie_FileSize;
                return false;
            }
        }

        return true;
    }
}