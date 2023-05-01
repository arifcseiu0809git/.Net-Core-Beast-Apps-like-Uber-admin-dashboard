using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

public class PaymentOptionModelValidator : AbstractValidator<PaymentOptionModel>
{
    public PaymentOptionModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Name'.");
            //.MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
            //.MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");
    }
}