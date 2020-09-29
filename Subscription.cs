using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Server;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotChocoloteSubscriptionTest
{
    public class Subscription
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Subscription> _logger;

        public Subscription(IServiceProvider serviceProvider, ILogger<Subscription> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [SubscribeAndResolve]
        public async Task<IAsyncEnumerable<TestEvent>> OnTestEvent([Service] ITopicEventReceiver eventReceiver, [Service]IResolverContext context, [State("DataStopCancellationToken")]CancellationToken dataStopToken, CancellationToken cancellationToken)
        {
            string key = Guid.NewGuid().ToString();
            _logger.LogInformation("Starting subcription for {key}", key);

            dataStopToken.Register(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CancellationToken.Register");

                logger.LogError("Cancel called for {key}", key);
            });

            _ = Task.Factory.StartNew(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var eventSender = scope.ServiceProvider.GetRequiredService<ITopicEventSender>();
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(OnTestEvent));

                logger.LogWarning("Starting publishing for {key}", key);

                int count = 0;

                try
                {
                    while (dataStopToken.IsCancellationRequested == false)
                    {
                        var testEvent = new TestEvent()
                        {
                            Key = key,
                            Count = count++,
                            DateTime = DateTime.Now,
                        };

                        logger.LogInformation("Publishing {key} {count} {dateTime}", testEvent.Key, testEvent.Count, testEvent.DateTime);

                        await eventSender.SendAsync(new TestEventTopic(key), testEvent, dataStopToken);

                        await Task.Delay(1000, dataStopToken);
                    }
                }
                catch (TaskCanceledException) { }

                logger.LogWarning("Stopped publishing for {key}", key);
            }, TaskCreationOptions.LongRunning);

            return (IAsyncEnumerable<TestEvent>) await eventReceiver
                .SubscribeAsync<TestEventTopic, TestEvent>(new TestEventTopic(key), cancellationToken);
        }
    }
}
