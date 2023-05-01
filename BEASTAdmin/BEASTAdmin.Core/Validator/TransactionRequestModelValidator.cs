using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

public class TransactionRequestModelValidator : AbstractValidator<TransactionRequestModel>
{
    public TransactionRequestModelValidator()
    {
        RuleFor(p => p.APIEndPointRequestData)
            .NotEmpty().WithMessage("Please enter 'API End Point Request Data'.")
            .MinimumLength(10).WithMessage("Minimum length of 'API End Point Request Data' is 10 characters.")
            .MaximumLength(1000).WithMessage("Maximum length of 'API End Point Request Data' is 1000 characters.");
    }
}