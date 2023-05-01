using FluentValidation;
using BEASTAPI.Core.Model.Driver;

namespace BEASTAPI.Core.Validator.DriverValidator;

public class DriverModelValidator : AbstractValidator<DriverModel>
{
    public DriverModelValidator()
    {
        RuleFor(p => p.FirstName)
             .NotEmpty().WithMessage("Please enter 'First Name'.")
             .MinimumLength(3).WithMessage("Minimum length of 'First Name' is 3 characters.")
             .MaximumLength(150).WithMessage("Maximum length of 'First Name' is 150 characters.");

        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Please enter 'Last Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Last Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Last Name' is 150 characters.");

        RuleFor(p => p.MobileNumber)
           .NotEmpty().WithMessage("Please enter 'Mobile Number'.")
           .MinimumLength(11).WithMessage("Minimum length of 'Mobile Number' is 11 characters.")
           .MaximumLength(13).WithMessage("Maximum length of 'Mobile Number' is 13 characters.");

        RuleFor(p => p.DrivingLicenseNo)
           .NotEmpty().WithMessage("Please enter 'Driving License No'.")
           .MinimumLength(3).WithMessage("Minimum length of 'Driving License No' is 3 characters.")
           .MaximumLength(150).WithMessage("Maximum length of 'Driving License No' is 150 characters.");

    }
}