using FluentValidation;
using BEASTAdmin.Core.Model.Vehicle;

namespace BEASTAdmin.Core.Validator.Vehicle;

public class VehicleFareValidator : AbstractValidator<VehicleFareModel>
{
    public VehicleFareValidator()
    {
        RuleFor(p => p.BaseFare).NotNull().WithMessage("Base Fare is required");
    }
}