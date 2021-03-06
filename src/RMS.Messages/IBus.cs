﻿using System.Threading.Tasks;

namespace RMS.Messages
{
    public interface IBus
    {
        Task<TCommandResult> ExecuteAsync<TCommandResult>(ICommand<TCommandResult> command);

        Task<TQueryResult> QueryAsync<TQueryResult>(IQuery<TQueryResult> queryModel);
    }
}