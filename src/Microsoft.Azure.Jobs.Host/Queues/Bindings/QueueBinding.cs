﻿using System;
using System.IO;
using Microsoft.Azure.Jobs.Host.Bindings;
using Microsoft.Azure.Jobs.Host.Converters;
using Microsoft.Azure.Jobs.Host.Protocols;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Azure.Jobs.Host.Queues.Bindings
{
    internal class QueueBinding : IBinding
    {
        private readonly IArgumentBinding<CloudQueue> _argumentBinding;
        private readonly CloudQueue _queue;
        private readonly IObjectToTypeConverter<CloudQueue> _converter;

        public QueueBinding(IArgumentBinding<CloudQueue> argumentBinding, CloudQueue queue)
        {
            _argumentBinding = argumentBinding;
            _queue = queue;
            _converter = CreateConverter(queue);
        }

        private static IObjectToTypeConverter<CloudQueue> CreateConverter(CloudQueue queue)
        {
            return new CompositeObjectToTypeConverter<CloudQueue>(
                new OutputConverter<CloudQueue>(new IdentityConverter<CloudQueue>()),
                new OutputConverter<string>(new StringToCloudQueueConverter(queue)));
        }

        public string QueueName
        {
            get { return _queue.Name; }
        }

        private FileAccess Access
        {
            get
            {
                return _argumentBinding.ValueType == typeof(CloudQueue)
                    ? FileAccess.ReadWrite : FileAccess.Write;
            }
        }

        public IValueProvider Bind(BindingContext context)
        {
            return Bind(_queue, context);
        }

        private IValueProvider Bind(CloudQueue value, ArgumentBindingContext context)
        {
            return _argumentBinding.Bind(value, context);
        }

        public IValueProvider Bind(object value, ArgumentBindingContext context)
        {
            CloudQueue queue = null;

            if (!_converter.TryConvert(value, out queue))
            {
                throw new InvalidOperationException("Unable to convert value to CloudQueue.");
            }

            return Bind(queue, context);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new QueueParameterDescriptor
            {
                QueueName = _queue.Name,
                Access = Access
            };
        }
    }
}
