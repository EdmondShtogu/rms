﻿using RMS.Core;
using System.Threading.Tasks;

namespace RMS.Messages
{
    public sealed class InMemoryBus : IBus
    {
        private readonly IHandlerResolver _handlerResolver;

        public InMemoryBus(IHandlerResolver handlerResolver)
        {
            _handlerResolver = handlerResolver;
        }

        public async Task<TCommandResult> ExecuteAsync<TCommandResult>(ICommand<TCommandResult> command)
        {
            var handlerInstance = (dynamic)_handlerResolver.ResolveCommandHandler(command, typeof(ICommandHandler<,>));

            var result = await handlerInstance.HandleAsync((dynamic)command);
            if (result.IsSuccess)
            {
                return result.Value;
            }

            throw new GeneralException(result.Error);
        }

        public async Task<TQueryResult> QueryAsync<TQueryResult>(IQuery<TQueryResult> query)
        {
            var handlerInstance = (dynamic)_handlerResolver.ResolveQueryHandler(query, typeof(IQueryHandler<,>));

            var result = await handlerInstance.HandleAsync((dynamic)query);
            if (result.IsSuccess)
            {
                return result.Value;
            }
            throw new GeneralException(result.Error);
        }
    }
}