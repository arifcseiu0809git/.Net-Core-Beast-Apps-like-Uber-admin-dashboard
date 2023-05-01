using BEASTAdmin.Core.Resources;
using BEASTAdmin.Core.Validator;

namespace BEASTAdmin.UI.Pages.Driver;

public partial class DocumentUpLoad
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
        var validationResult = await new DocumentModelValidator().ValidateAsync(DocumentModel);
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
		if (DocumentImage != null)
		{
			string[] supportedImageTypes = _config["SiteSettings:SupportedImageTypes"].Split(',');
			if (!supportedImageTypes.Contains(Path.GetExtension(DocumentImage.FileName).Substring(1)))
			{
				ErrorMessage = ValidationMessages.Pie_FileExtension;
				return false;
			}

			if ((DocumentImage.Length / 1024) > Convert.ToInt32(300)) //upto 300kb
			{
				ErrorMessage = ValidationMessages.Pie_FileSize;
				return false;
			}
		}

		return true;
	}

}