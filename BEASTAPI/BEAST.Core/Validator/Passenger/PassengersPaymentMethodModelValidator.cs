using FluentValidation;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Core.Validator;

public class PassengersPaymentMethodModelValidator : AbstractValidator<PassengersPaymentMethodModel>
{
    public PassengersPaymentMethodModelValidator()
    {
        RuleFor(p => p.PaymentMethodName)
            .NotEmpty().WithMessage("Please enter 'Payment Method Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Payment Method Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Payment Method Name' is 150 characters.");

        RuleFor(p => p.PaymentMethodType)
            .NotEmpty().WithMessage("Please enter 'PaymentMethodType'.");

        RuleFor(p => p.Password)
             .NotEmpty().WithMessage("Please enter 'Password'.");

        RuleFor(p => p.AccountNumber)
            .NotEmpty().WithMessage("Please enter 'Account Number'.");

        RuleFor(p => p.AccountType)
            .NotEmpty().WithMessage("Please enter 'Account Type'.");

        RuleFor(p => p.ExpiredOnMonth)
            .NotEmpty().WithMessage("Please enter 'Expired On Month'.");

        RuleFor(p => p.ExpiredOnYear)
             .NotEmpty().WithMessage("Please enter 'Expired On Year'.");

        RuleFor(p => p.Cvv)
            .NotEmpty().WithMessage("Please enter 'Cvv'.");

        RuleFor(p => p.CountryName)
            .NotEmpty().WithMessage("Please enter 'Country Name'.");
    }
}