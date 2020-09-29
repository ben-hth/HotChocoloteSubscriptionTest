using HotChocolate.AspNetCore.Subscriptions.Messages;
using HotChocolate.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotChocoloteSubscriptionTest
{
    public class NewDataStartMessageHandler
        : MessageHandler<DataStartMessage>
    {
        private readonly DataStartMessageHandler _dataStartMessageHandler;

        public NewDataStartMessageHandler(DataStartMessageHandler dataStartMessageHandler)
        {
            _dataStartMessageHandler = dataStartMessageHandler ?? throw new ArgumentNullException(nameof(dataStartMessageHandler));
        }

        protected async override Task HandleAsync(ISocketConnection connection, DataStartMessage message, CancellationToken cancellationToken)
        {
            await _dataStartMessageHandler.HandleAsync(connection, message, cancellationToken);
        }
    }
}
