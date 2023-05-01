using FluentValidation;
using BEASTAPI.Core.Model.Common;

namespace BEASTAPI.Core.Validator.Common;

public class MessageModelValidator : AbstractValidator<MessageModel>
{
    public MessageModelValidator()
    {
        RuleFor(p => p.TitleText)
            .NotEmpty().WithMessage("Please enter 'Message Title'.")
            .MaximumLength(250).WithMessage("Maximum length of 'Message NaTitleme' is 250 characters.");
    }
}