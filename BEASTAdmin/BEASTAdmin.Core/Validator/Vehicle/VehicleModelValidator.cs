using FluentValidation;
using BEASTAdmin.Core.Model.Vehicle;

namespace BEASTAdmin.Core.Validator.Vehicle;

public class VehicleModelValidator : AbstractValidator<VehicleModel>
{
    public VehicleModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");

        RuleFor(p => p.Year)
            .GreaterThanOrEqualTo(1990).WithMessage("Year Should be greater 1990.");

        RuleFor(p => p.CubicCapacity)
            .GreaterThanOrEqualTo(50).WithMessage("Cubic Capacity  Should be Minimum 50 CC.");

        RuleFor(p => p.VehicleBrandId)
            .NotEmpty().WithMessage("Please Select 'Brand'.");

    }
}