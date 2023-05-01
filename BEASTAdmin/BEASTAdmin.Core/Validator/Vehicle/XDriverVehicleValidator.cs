using FluentValidation;
using BEASTAdmin.Core.Model.Vehicle;

namespace BEASTAdmin.Core.Validator.Vehicle;

public class XDriverVehicleValidator : AbstractValidator<XDriverVehicleUpsertModel>
{
    public XDriverVehicleValidator()
    {
        RuleFor(p => p.UserId).NotEmpty().WithMessage("Please select driver.");
		RuleFor(p => p.VehicleId).NotEmpty().WithMessage("Please select vehicle.");
	}
}