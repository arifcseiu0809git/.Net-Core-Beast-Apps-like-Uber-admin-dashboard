using FluentValidation;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Validator.Common;

public class TripModelValidator : AbstractValidator<TripModel>
{
    public TripModelValidator()
    {
        RuleFor(p => p.StartLocationName)
            .NotEmpty().WithMessage("Please enter 'Start Location Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Start Location Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Start Location Name' is 150 characters.");
    }
}