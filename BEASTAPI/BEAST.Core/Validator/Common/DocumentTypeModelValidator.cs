using FluentValidation;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Validator.Common;

public class DocumentTypeModelValidator : AbstractValidator<DocumentTypeModel>
{
    public DocumentTypeModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Document Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Document Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Document Name' is 150 characters.");
    }
}