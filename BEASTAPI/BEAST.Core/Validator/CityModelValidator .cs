using FluentValidation;
using BEASTAPI.Core.Model;

namespace BEASTAPI.Core.Validator;

public class CityModelValidator : AbstractValidator<CityModel>
{
    public CityModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Name'.");
    }
}