﻿using FluentValidation;
using BEASTAdmin.Core.Model;

namespace BEASTAdmin.Core.Validator;

public class CountryModelValidator : AbstractValidator<CountryModel>
{
    public CountryModelValidator()
    {
        RuleFor(p => p.CountryName)
            .NotEmpty().WithMessage("Please enter 'Country Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Country Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Country Name' is 150 characters.");
    }
}