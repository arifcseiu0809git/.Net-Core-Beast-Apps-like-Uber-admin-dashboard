using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

public class SavedAddressModelValidator : AbstractValidator<SavedAddressModel>
{
    public SavedAddressModelValidator()
    {
        RuleFor(p => p.HomeAddress)
            .NotEmpty().WithMessage("Please enter 'Home Address'.");
            //.MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
            //.MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");
    }
}