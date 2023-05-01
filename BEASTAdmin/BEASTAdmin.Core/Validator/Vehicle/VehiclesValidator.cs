using FluentValidation;
using BEASTAdmin.Core.Model.Vehicle;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Drawing;

namespace BEASTAdmin.Core.Validator.Vehicle;

public class VehiclesValidator : AbstractValidator<VehiclesList>
{
    public VehiclesValidator()
    {
        RuleFor(p => p.VehicleBrandId)
         .NotEmpty().WithMessage("Please Select a 'Brand'.");
        RuleFor(p => p.VehicleModelId)
         .NotEmpty().WithMessage("Please Select a 'Model'.");
        RuleFor(p => p.VehicleTypeId)
         .NotEmpty().WithMessage("Please Select a 'Type'.");
        RuleFor(p => p.StatusId)
         .NotEmpty().WithMessage("Please Select a 'Status'.");
        RuleFor(p => p.RegistrationNo)
        .NotEmpty().WithMessage("Please Enter a 'Registration No'.");
        RuleFor(p => p.Color)
        .NotEmpty().WithMessage("Please Enter a 'Color'.");
        RuleFor(p => p.FuelTypeId)
        .NotEmpty().WithMessage("Please Enter a 'Fuel'.");
        RuleFor(p => p.EngineNo)
        .NotEmpty().WithMessage("Please Enter a 'Engine No'.");
        RuleFor(p => p.RegistrationDate)
        .NotEmpty().WithMessage("Please Enter a 'Registration Date'.");
        RuleFor(p => p.AuthorityId)
        .NotEmpty().WithMessage("Please Enter a 'Authority'.");
        RuleFor(p => p.Seat)
            .GreaterThanOrEqualTo(0).WithMessage("Seat can not be negative.");
    }
}

