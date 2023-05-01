using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

public class TripInitialModelValidator : AbstractValidator<TripInitialModel>
{
    public TripInitialModelValidator()
    {
        RuleFor(p => p.OriginLatitude)
            .NotEmpty().WithMessage("Please enter 'Origin Latitude'.");
    }
}