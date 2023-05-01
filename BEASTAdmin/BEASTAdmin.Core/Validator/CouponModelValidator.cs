using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

public class CouponModelValidator : AbstractValidator<CouponModel>
{
    public CouponModelValidator()
    {
        RuleFor(p => p.CouponCode)
            .NotEmpty().WithMessage("Please enter 'Coupon Code'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Coupon Code' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Coupon Code' is 150 characters.");
    }
}