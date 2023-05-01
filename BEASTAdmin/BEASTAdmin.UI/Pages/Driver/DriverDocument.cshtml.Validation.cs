﻿
using BEASTAdmin.Core.Resources;
using BEASTAdmin.Core.Validator;

namespace BEASTAdmin.UI.Pages.Driver;

public partial class DriverDocument
{
    private async Task<bool> ValidatePost()
    {
        bool IsValid = true;

        IsValid = await ValidateModel();
     //   if (IsValid) IsValid = ValidateImage();

        return IsValid;
    }

    private async Task<bool> ValidateModel()
    {
        var validationResult = await new DocumentModelValidator().ValidateAsync(DocumentModel);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ErrorMessage += error.ErrorMessage + "<br>";
            return false;
        }

        return true;
    }
   
}