using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

public class TransactionDetailModelValidator : AbstractValidator<TransactionDetailModel>
{
    public TransactionDetailModelValidator()
    {
        RuleFor(p => p.TransactionAmount)
            .GreaterThan(0).WithMessage("Please Enter 'Transaction Amount'.");
    }
}