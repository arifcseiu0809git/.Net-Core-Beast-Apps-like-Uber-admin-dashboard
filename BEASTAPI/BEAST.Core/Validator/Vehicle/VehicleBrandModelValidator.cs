using FluentValidation;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Validator.Vehicle;

public class VehicleBrandModelValidator : AbstractValidator<VehicleBrandModel>
{
    public VehicleBrandModelValidator()
    {
        RuleFor(p => p.Name)
             .NotEmpty().WithMessage("Please enter 'Name'.")
             .MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
             .MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");

    }
}