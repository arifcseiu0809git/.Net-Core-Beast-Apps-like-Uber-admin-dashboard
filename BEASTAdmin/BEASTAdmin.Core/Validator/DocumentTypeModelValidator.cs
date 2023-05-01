using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

public class DocumentTypeModelValidator : AbstractValidator<DocumentTypeModel>
{
    public DocumentTypeModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Name'.");

		RuleFor(p => p.DocumentFor)
	   .NotEmpty().WithMessage("Please Select 'Document for'.");

	}
}