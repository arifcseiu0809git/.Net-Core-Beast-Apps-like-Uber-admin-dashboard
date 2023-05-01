using FluentValidation;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Validator.Vehicle;

public class VehicleCurrentLocationValidator : AbstractValidator<VehicleCurrentLocationModel>
{
    public VehicleCurrentLocationValidator()
    {
        RuleFor(p => p.GoingDirection)
             .NotEmpty().WithMessage("Please enter 'GoingDirection'.")
             .MinimumLength(3).WithMessage("Minimum length of 'GoingDirection' is 2 characters.")
             .MaximumLength(150).WithMessage("Maximum length of 'GoingDirection' is 150 characters.");

        RuleFor(p => p.Latitude)
			.NotEmpty().WithMessage("Please Enter 'Latitude'.");

        RuleFor(p => p.Longitude)
            .NotEmpty().WithMessage("Please Enter 'Longitude'.");

        RuleFor(p => p.LastUpdateAt)
            .NotEmpty().WithMessage("Please enter 'LastUpdateAt'.");
    }
}