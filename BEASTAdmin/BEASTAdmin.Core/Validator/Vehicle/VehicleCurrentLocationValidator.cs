using FluentValidation;
using BEASTAdmin.Core.Model.Vehicle;
using System.ComponentModel.DataAnnotations;

namespace BEASTAdmin.Core.Validator.Vehicle;

public class VehicleCurrentLocationValidator : AbstractValidator<VehicleCurrentLocationModel>
{
    public VehicleCurrentLocationValidator()
    {
        RuleFor(p => p.Latitude)
            .NotEmpty().WithMessage("Please enter Latitude");
		RuleFor(p => p.Longitude)
		   .NotEmpty().WithMessage("Please enter Longitude");
		RuleFor(p => p.GoingDirection)
		   .NotEmpty().WithMessage("Please enter GoingDirection");
	}
}