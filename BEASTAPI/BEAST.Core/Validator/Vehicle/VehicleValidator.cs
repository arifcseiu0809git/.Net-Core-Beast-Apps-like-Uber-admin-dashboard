using FluentValidation;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Validator.Vehicle;

public class VehicleValidator : AbstractValidator<BEASTAPI.Core.Model.Vehicle.Vehicle>
{
    public VehicleValidator()
    {
        RuleFor(p => p.VehicleBrandId)
             .NotEmpty().WithMessage("Please enter 'Brand'.");
        RuleFor(p => p.VehicleModelId)
			.NotEmpty().WithMessage("Please Enter 'Model'.");
        RuleFor(p => p.Authority)
            .NotEmpty().WithMessage("Please Enter 'Authority'.")        
            .MinimumLength(3).WithMessage("Minimum length of 'Authority' is 2 characters.")
			.MaximumLength(150).WithMessage("Maximum length of 'Authority' is 150 characters.");
		RuleFor(p => p.Weight)
            .NotEmpty().WithMessage("Please enter 'Weight'.");
		RuleFor(p => p.Color)
			.NotEmpty().WithMessage("Please enter 'Color'.");
		RuleFor(p => p.FitnessExpiredOn)
			.NotEmpty().WithMessage("Please enter 'Fitness Expired Date'.");
		RuleFor(p => p.InsuranceExpiresOn)
			.NotEmpty().WithMessage("Please enter 'Insurance Expired Date'.");
		RuleFor(p => p.RegistrationNo)
			.NotEmpty().WithMessage("Please enter 'Registration No'.");
	}
}