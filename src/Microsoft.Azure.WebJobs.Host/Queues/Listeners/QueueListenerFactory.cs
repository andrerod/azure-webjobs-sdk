﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Storage.Queue;
using Microsoft.Azure.WebJobs.Host.Timers;

namespace Microsoft.Azure.WebJobs.Host.Queues.Listeners
{
    internal class QueueListenerFactory : IListenerFactory
    {
        private static string poisonQueueSuffix = "-poison";

        private readonly IStorageQueue _queue;
        private readonly IStorageQueue _poisonQueue;
        private readonly IQueueConfiguration _queueConfiguration;
        private readonly IBackgroundExceptionDispatcher _backgroundExceptionDispatcher;
        private readonly IContextSetter<IMessageEnqueuedWatcher> _messageEnqueuedWatcherSetter;
        private readonly ISharedContextProvider _sharedContextProvider;
        private readonly TraceWriter _trace;
        private readonly ITriggeredFunctionExecutor _executor;

        public QueueListenerFactory(IStorageQueue queue,
            IQueueConfiguration queueConfiguration,
            IBackgroundExceptionDispatcher backgroundExceptionDispatcher,
            IContextSetter<IMessageEnqueuedWatcher> messageEnqueuedWatcherSetter,
            ISharedContextProvider sharedContextProvider,
            TraceWriter trace,
            ITriggeredFunctionExecutor executor)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            if (queueConfiguration == null)
            {
                throw new ArgumentNullException(nameof(queueConfiguration));
            }

            if (backgroundExceptionDispatcher == null)
            {
                throw new ArgumentNullException(nameof(backgroundExceptionDispatcher));
            }

            if (messageEnqueuedWatcherSetter == null)
            {
                throw new ArgumentNullException(nameof(messageEnqueuedWatcherSetter));
            }

            if (sharedContextProvider == null)
            {
                throw new ArgumentNullException(nameof(sharedContextProvider));
            }

            if (trace == null)
            {
                throw new ArgumentNullException(nameof(trace));
            }

            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            _queue = queue;
            _poisonQueue = CreatePoisonQueueReference(queue.ServiceClient, queue.Name);
            _queueConfiguration = queueConfiguration;
            _backgroundExceptionDispatcher = backgroundExceptionDispatcher;
            _messageEnqueuedWatcherSetter = messageEnqueuedWatcherSetter;
            _sharedContextProvider = sharedContextProvider;
            _trace = trace;
            _executor = executor;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Task<IListener> CreateAsync(CancellationToken cancellationToken)
        {
            QueueTriggerExecutor triggerExecutor = new QueueTriggerExecutor(_executor);

            IDelayStrategy delayStrategy = new RandomizedExponentialBackoffStrategy(QueuePollingIntervals.Minimum, _queueConfiguration.MaxPollingInterval);
            
            SharedQueueWatcher sharedWatcher = _sharedContextProvider.GetOrCreate<SharedQueueWatcher>(
                new SharedQueueWatcherFactory(_messageEnqueuedWatcherSetter));

            IListener listener = new QueueListener(_queue, _poisonQueue, triggerExecutor, delayStrategy,
                _backgroundExceptionDispatcher, _trace, sharedWatcher, _queueConfiguration);

            return Task.FromResult(listener);
        }

        private static IStorageQueue CreatePoisonQueueReference(IStorageQueueClient client, string name)
        {
            Debug.Assert(client != null);

            // Only use a corresponding poison queue if:
            // 1. The poison queue name would be valid (adding "-poison" doesn't make the name too long), and
            // 2. The queue itself isn't already a poison queue.

            if (name == null || name.EndsWith(poisonQueueSuffix, StringComparison.Ordinal))
            {
                return null;
            }

            string possiblePoisonQueueName = name + poisonQueueSuffix;

            if (!QueueClient.IsValidQueueName(possiblePoisonQueueName))
            {
                return null;
            }

            return client.GetQueueReference(possiblePoisonQueueName);
        }
    }
}
