using FluentValidation;
using BEASTAdmin.Core.Model;
namespace BEASTAdmin.Core.Validator;

public class TransactionResponseModelValidator : AbstractValidator<TransactionResponseModel>
{
    public TransactionResponseModelValidator()
    {
        RuleFor(p => p.APIResponseData)
            .NotEmpty().WithMessage("Please enter 'API Response Data'.")
            .MinimumLength(10).WithMessage("Minimum length of 'API Response Data' is 10 characters.")
            .MaximumLength(1000).WithMessage("Maximum length of 'API Response Data' is 1000 characters.");
    }
}