using FluentValidation;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Core.Validator;

public class PassengersLocationModelValidator : AbstractValidator<PassengersLocationModel>
{
    public PassengersLocationModelValidator()
    {
        RuleFor(p => p.LandmarkName)
            .NotEmpty().WithMessage("Please enter 'Landmark Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Landmark Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Landmark Name' is 150 characters.");

        RuleFor(p => p.LandmarkType)
            .NotEmpty().WithMessage("Please enter 'Landmark Type'.");

        RuleFor(p => p.ZipCode)
             .NotEmpty().WithMessage("Please enter 'ZipCode'.");

        RuleFor(p => p.MapLatitude)
            .NotEmpty().WithMessage("Please enter 'Map Latitude'.");

        RuleFor(p => p.MapLongitude)
            .NotEmpty().WithMessage("Please enter 'Map Longitude'.");
    }
}