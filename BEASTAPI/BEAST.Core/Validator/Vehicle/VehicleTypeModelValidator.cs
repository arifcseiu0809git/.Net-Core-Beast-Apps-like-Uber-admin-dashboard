using FluentValidation;
using BEASTAPI.Core.Model.Vehicle;

namespace BEASTAPI.Core.Validator.Vehicle;

public class VehicleTypeModelValidator : AbstractValidator<VehicleTypeModel>
{
    public VehicleTypeModelValidator()
    {
        RuleFor(p => p.Name)
             .NotEmpty().WithMessage("Please enter 'Name'.")
             .MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
             .MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");

        RuleFor(p => p.UnitPricePerKm)
            .GreaterThan(0).WithMessage("Please Enter 'Unit Price per Km'.");

        RuleFor(p => p.WaitingTimeCostPerMin)
            .GreaterThan(0).WithMessage("Please Enter 'Waiting time cost per min'.");

        RuleFor(p => p.ImageName)
            .NotEmpty().WithMessage("Please enter 'Image Name'.");

        RuleFor(p => p.Descriptions)
            .NotEmpty().WithMessage("Please enter 'Descriptions'.");

    }
}