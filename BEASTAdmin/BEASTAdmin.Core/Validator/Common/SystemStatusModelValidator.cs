using FluentValidation;
using BEASTAdmin.Core.Model.Common;

namespace BEASTAdmin.Core.Validator.Common;

public class SystemStatusModelValidator : AbstractValidator<SystemStatusModel>
{
    public SystemStatusModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");
    }
}