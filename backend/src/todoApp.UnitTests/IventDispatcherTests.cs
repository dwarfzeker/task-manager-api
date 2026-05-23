using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using System;


using todoApp.Core.Interfaces;
using todoApp.Core.Events;
using todoApp.Data.EventDispatcher;

namespace todoApp.UnitTests;

public class IventDispatcherTests
{
    private readonly Mock<ILogger<EventDispatcher>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public IventDispatcherTests()
    {
        _loggerMock = new Mock<ILogger<EventDispatcher>>();
        var mockEventDispatcher = new Mock<IEventDispatcher>();
        _serviceProviderMock = new Mock<IServiceProvider>();
    }

    [Fact]
    public async Task DspatchAsync_ShouldCallAllHandlers()
    {
        var Handler1Mock = new Mock<IEventHandler<TaskCompletedEvent>>();
        var Handler2Mock = new Mock<IEventHandler<TaskCompletedEvent>>();
        var handlers = new List<IEventHandler<TaskCompletedEvent>>
        {
            Handler1Mock.Object,
            Handler2Mock.Object
        };
        _serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns<Type>(type => 
        {
            if (type == typeof(IEnumerable<IEventHandler<TaskCompletedEvent>>))
                return handlers;
            return null;
        });
        var dispatcher = new EventDispatcher(_serviceProviderMock.Object, _loggerMock.Object);
        var @event = new TaskCompletedEvent("task1", "user1", DateTime.UtcNow);
        await dispatcher.DispatchAsync(@event);
        Handler1Mock.Verify(handler => handler.HandleAsync(@event), Times.Once);
        Handler2Mock.Verify(handler => handler.HandleAsync(@event), Times.Once);
        
        

    }

    [Fact]
    public async Task DispatchAsync_WhereHandlerThrows_ShouldCallOtherHandlers()
    {
        var handler1Mock = new Mock<IEventHandler<TaskCompletedEvent>>();
        var handler2Mock = new Mock<IEventHandler<TaskCompletedEvent>>();
        handler1Mock.Setup(handler => handler.HandleAsync(It.IsAny<TaskCompletedEvent>()))
            .ThrowsAsync(new Exception("HandlerError"));
        var handlers = new List<IEventHandler<TaskCompletedEvent>>
        {
            handler1Mock.Object,
            handler2Mock.Object
        };
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<TaskCompletedEvent>>))).Returns(handlers);
        var @event = new TaskCompletedEvent("task1", "user1", DateTime.UtcNow);
        var dispatcher = new EventDispatcher(_serviceProviderMock.Object, _loggerMock.Object);
        await dispatcher.DispatchAsync(@event);
        
        handler1Mock.Verify(handler => handler.HandleAsync(@event), Times.Once); // должен упасть
        handler2Mock.Verify(handler => handler.HandleAsync(@event), Times.Once); // все равно будет вызван
    }
    
}