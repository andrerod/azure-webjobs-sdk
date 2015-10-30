﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Host.Converters;
using Microsoft.Azure.WebJobs.Host.Storage.Queue;

namespace Microsoft.Azure.WebJobs.Host.Queues.Bindings
{
    internal class StringToStorageQueueMessageConverter : IConverter<string, IStorageQueueMessage>
    {
        private readonly IStorageQueue _queue;

        public StringToStorageQueueMessageConverter(IStorageQueue queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }

            _queue = queue;
        }

        public IStorageQueueMessage Convert(string input)
        {
            if (input == null)
            {
                throw new InvalidOperationException("A queue message cannot contain a null string instance.");
            }

            return _queue.CreateMessage(input);
        }
    }
}
