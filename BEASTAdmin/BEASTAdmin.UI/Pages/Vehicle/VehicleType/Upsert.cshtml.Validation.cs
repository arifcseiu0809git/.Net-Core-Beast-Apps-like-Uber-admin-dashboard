﻿using BEASTAdmin.Core.Resources;
using BEASTAdmin.Core.Validator.Vehicle;

namespace BEASTAdmin.UI.Pages.VehicleType;

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
        var validationResult = await new VehicleTypeModelValidator().ValidateAsync(VehicleType);
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
        if (VehicleTypeImage != null)
        {
            string[] supportedImageTypes = _config["SiteSettings:SupportedImageTypes"].Split(',');
            if (!supportedImageTypes.Contains(Path.GetExtension(VehicleTypeImage.FileName).Substring(1)))
            {
                ErrorMessage = ValidationMessages.Pie_FileExtension;
                return false;
            }

            if ((VehicleTypeImage.Length / 1024) > Convert.ToInt32(_config["SiteSettings:MaxImageSize"]))
            {
                ErrorMessage = ValidationMessages.Pie_FileSize;
                return false;
            }
        }

        return true;
    }
}