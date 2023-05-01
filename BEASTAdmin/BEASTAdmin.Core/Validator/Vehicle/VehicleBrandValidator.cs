using FluentValidation;
using BEASTAdmin.Core.Model.Vehicle;

namespace BEASTAdmin.Core.Validator.Vehicle;

public class VehicleBrandValidator : AbstractValidator<VehicleBrandModel>
{
    public VehicleBrandValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");

        
    }
}