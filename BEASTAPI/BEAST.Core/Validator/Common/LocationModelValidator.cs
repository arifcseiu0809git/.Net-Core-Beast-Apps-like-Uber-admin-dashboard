using FluentValidation;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Validator.Common;

public class LocationModelValidator : AbstractValidator<LocationModel>
{
    public LocationModelValidator()
    {
        RuleFor(p => p.LandmarkName)
            .NotEmpty().WithMessage("Please enter 'Landmark Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Landmark Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Landmark Name' is 150 characters.");
    }
}