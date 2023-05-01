using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

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