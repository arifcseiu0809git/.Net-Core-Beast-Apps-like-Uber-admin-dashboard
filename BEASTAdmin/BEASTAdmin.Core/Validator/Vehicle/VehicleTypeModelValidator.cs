using FluentValidation;
using BEASTAdmin.Core.Model.Vehicle;

namespace BEASTAdmin.Core.Validator.Vehicle;

public class VehicleTypeModelValidator : AbstractValidator<VehicleTypeModel>
{
    public VehicleTypeModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");

        RuleFor(p => p.WaitingTimeCostPerMin)
            .GreaterThanOrEqualTo(0).WithMessage("Waiting Time Cost Per Min can not be negative.");

        RuleFor(p => p.UnitPricePerKm)
            .GreaterThanOrEqualTo(0).WithMessage("Unit Price Per Km can not be negative.");

        RuleFor(p => p.Descriptions)
            .NotEmpty().WithMessage("Please enter 'Description'.")
            .MinimumLength(20).WithMessage("Minimum length of 'Description' is 20 characters.")
            .MaximumLength(4000).WithMessage("Maximum length of 'Description' is 4000 characters.");
    }
}