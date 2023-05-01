using FluentValidation;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Validator.Common;

public class SystemStatusModelValidator : AbstractValidator<SystemStatusModel>
{
    public SystemStatusModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'System Status Name' is 150 characters.");
    }
}