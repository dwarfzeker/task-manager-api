using todoApp.Core.Interfaces;

namespace todoApp.Data.EventDispatcher;

public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventDispatcher> _logger;
    public EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
    {
        _logger.LogInformation($"Dispatching event {@event}");

        var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();
        var tasks = handlers.Select(async handler =>
        {

            try
            {
                await handler.HandleAsync(@event);
                _logger.LogInformation($"Handled event {@event}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"looks like an error happened {ex.Message} in handler {handler.GetType().Name}");
            }

        });

        await Task.WhenAll(tasks);
    }
}