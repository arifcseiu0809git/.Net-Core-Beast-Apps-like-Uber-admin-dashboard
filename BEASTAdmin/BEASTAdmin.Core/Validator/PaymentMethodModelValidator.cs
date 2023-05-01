using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

public class PaymentMethodModelValidator : AbstractValidator<PaymentMethodModel>
{
    public PaymentMethodModelValidator()
    {
        RuleFor(p => p.PaymentType)
            .NotEmpty().WithMessage("Please enter 'Payment Type'.");
            //.MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
            //.MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");
    }
}