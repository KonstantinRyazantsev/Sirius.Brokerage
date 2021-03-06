﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;

namespace BrokerageTests.InMemoryImplementations
{
    public class InMemoryPublishEndpoint : IPublishEndpoint
    {
        public List<object> Events { get; private set; } = new List<object>();
        public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
        {
            throw new NotImplementedException();
        }

        public Task Publish<T>(T message, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            throw new NotImplementedException();
        }

        public Task Publish(object message, CancellationToken cancellationToken = new CancellationToken())
        {
            Events.Add(message);

            return Task.CompletedTask;
        }

        public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task Publish(object message,
            Type messageType,
            IPipe<PublishContext> publishPipe,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task Publish<T>(object values, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
