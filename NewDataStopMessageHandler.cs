using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate.AspNetCore.Subscriptions.Messages;
using HotChocolate.Server;
using Microsoft.AspNetCore.Http;

namespace HotChocoloteSubscriptionTest
{
    public sealed class NewDataStopMessageHandler
        : MessageHandler<DataStopMessage>
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public NewDataStopMessageHandler(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        protected override Task HandleAsync(
            ISocketConnection connection,
            DataStopMessage message,
            CancellationToken cancellationToken)
        {
            var httpContext = _contextAccessor.HttpContext;
            connection.Subscriptions.Unregister(message.Id);

            var cts = (CancellationTokenSource)httpContext.Items["DataStopCancellationTokenSource"];
            httpContext.Items.Remove("DataStopCancellationTokenSource");
            cts.Cancel();

            //httpContext.Abort();
            return Task.CompletedTask;
        }
    }
}
