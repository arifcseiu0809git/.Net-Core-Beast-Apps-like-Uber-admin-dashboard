using FluentValidation;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Validator.Vehicle;

public class VehicleModelValidator : AbstractValidator<VehicleModel>
{
    public VehicleModelValidator()
    {
        RuleFor(p => p.Name)
             .NotEmpty().WithMessage("Please enter 'Vehicle Name'.")
             .MinimumLength(3).WithMessage("Minimum length of 'Registration No' is 3 characters.")
             .MaximumLength(150).WithMessage("Maximum length of 'Registration No' is 150 characters.");

    }
}