using FluentValidation;
using BEASTAPI.Core.Model.Map;

namespace BEASTAPI.Core.Validator.Map;

public class TripInitialModelValidator : AbstractValidator<TripInitialModel>
{
    public TripInitialModelValidator()
    {
        RuleFor(p => p.OriginLatitude)
            .NotEmpty().WithMessage("Please enter 'Origin Latitude'.");
    }
}