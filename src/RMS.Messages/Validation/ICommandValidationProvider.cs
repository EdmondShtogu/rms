﻿using RMS.Core.Domain;
using System;
using System.Threading.Tasks;

namespace RMS.Messages.Validation
{
    public interface ICommandValidationProvider
    {
        Task<ValidationResponse> ValidateAsync<TCommandResult>(ICommand<TCommandResult> command);
    }
    public class DefaultCommandValidationProvider : ICommandValidationProvider
    {
        public Task<ValidationResponse> ValidateAsync<TCommandResult>(ICommand<TCommandResult> command)
        {
            throw new NotImplementedException("Validation is not supported! Please provider an implementation for ICommandValidationProvider");
        }
    }
}