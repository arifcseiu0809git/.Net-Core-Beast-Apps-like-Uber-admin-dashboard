using FluentValidation;
using BEASTAPI.Core.Model;
using BEASTAPI.Core.Model.Passenger;

namespace BEASTAPI.Core.Validator;

public class TransactionModelValidator : AbstractValidator<TransactionModel>
{
    public TransactionModelValidator()
    {
        RuleFor(p => p.TotalBillAmount)
            .GreaterThan(0).WithMessage("Please Enter 'Bill Amount'.");

        RuleFor(p => p.BillDate)
            .NotEmpty().WithMessage("Please enter 'Bill Date'.");
    }
}