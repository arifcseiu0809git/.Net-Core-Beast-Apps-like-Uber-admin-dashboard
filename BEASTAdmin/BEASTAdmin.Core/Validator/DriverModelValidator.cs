using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

public class DriverModelValidator : AbstractValidator<DriverModel>
{
    public DriverModelValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("Please enter 'Name'.")
            .MinimumLength(1).WithMessage("Minimum length of 'Name' is 1 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");

        RuleFor(p => p.MobileNumber)
             .MinimumLength(10).WithMessage("Minimum length of 'Mobile Number' is 10 characters.");

		RuleFor(p => p.GenderId).MinimumLength(32).WithMessage("Please  Select Gender.");

		RuleFor(p => p.NID)
            .MinimumLength(13).WithMessage("NID  Should be Minimum 13 characters.");

        RuleFor(p => p.DrivingLicenseNo)
            .NotEmpty().WithMessage("Please Type 'Driving License No'.");
      
    }
}