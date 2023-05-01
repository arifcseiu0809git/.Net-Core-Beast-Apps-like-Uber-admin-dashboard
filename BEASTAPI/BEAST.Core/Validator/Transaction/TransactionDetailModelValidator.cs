using FluentValidation;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Core.Validator;

public class TransactionDetailModelValidator : AbstractValidator<TransactionDetailModel>
{
    public TransactionDetailModelValidator()
    {
        RuleFor(p => p.TransactionAmount)
            .GreaterThan(0).WithMessage("Please Enter 'Transaction Amount'.");
    }
}