using FluentValidation;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Validator.Common;

public class PricingModelValidator : AbstractValidator<PricingModel>
{
    public PricingModelValidator()
    {
       
        RuleFor(p => p.BaseFare)
            .GreaterThan(0).WithMessage("Please Enter 'Base Fare'.");

        RuleFor(p => p.BookingFee)
           .GreaterThanOrEqualTo(0).WithMessage("Please Enter 'Booking Fee'.");

        RuleFor(p => p.CostPerMin)
          .GreaterThanOrEqualTo(0).WithMessage("Please Enter 'Cost Per Min'.");

        RuleFor(p => p.CostPerKm)
          .GreaterThanOrEqualTo(0).WithMessage("Please Enter 'Cost Per Km'.");

        RuleFor(p => p.MinCharge)
          .GreaterThanOrEqualTo(0).WithMessage("Please Enter 'Min Charge'.");

        RuleFor(p => p.CancelFee)
         .GreaterThanOrEqualTo(0).WithMessage("Please Enter 'Cancel Fee'.");

    }
}