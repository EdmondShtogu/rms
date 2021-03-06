﻿using FluentValidation;
using RMS.Core;
using System;

namespace RMS.Application.Commands.RequestBC.Validators
{
    public class UpdateRequestStatusValidator : AbstractValidator<UpdateRequestStatus>
    {
        public UpdateRequestStatusValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty)
                .WithMessage(ErrorMessages.InvalidId);
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
        }
    }
}
