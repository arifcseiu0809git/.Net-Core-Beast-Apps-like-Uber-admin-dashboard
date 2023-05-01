using BEASTAPI.Core.Model;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEASTAPI.Core.Validator
{
    public class AdminEarningValidator : AbstractValidator<AdminEarning>
    {
        public AdminEarningValidator()
        {
            RuleFor(p => p.TripId).NotEmpty().WithMessage("Select a trip");
            RuleFor(p => p.PaymentTypeId).NotEmpty().WithMessage("Select a payment type");
            RuleFor(p => p.CommissionAmount).NotEmpty().WithMessage("Provide commission amount");
            RuleFor(p => p.CommissionReceiveDate).NotNull().WithMessage("Provide processing date");
            RuleFor(p => p.IsCommisionReceived).NotNull().WithMessage("Provide processing status");
        }
    }
}
