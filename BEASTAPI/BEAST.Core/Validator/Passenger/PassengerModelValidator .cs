using FluentValidation;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Core.Validator;

public class PassengerModelValidator : AbstractValidator<PassengerModel>
{
    public PassengerModelValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("Please enter 'First Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'First Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'First Name' is 150 characters.");

        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Please enter 'Last Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Last Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Last Name' is 150 characters.");

        RuleFor(p => p.Email)
           .NotEmpty().WithMessage("Please enter 'Email'.");

        RuleFor(p => p.DateOfBirth)
            .NotEmpty().WithMessage("Please enter 'Birth Date'.");

        RuleFor(p => p.Gender)
            .NotEmpty().WithMessage("Please enter 'Gender'.");

        RuleFor(p => p.MobileNumber)
            .NotEmpty().WithMessage("Please enter 'Mobile Number'.");

        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("Please enter 'Password'.");
    }
}