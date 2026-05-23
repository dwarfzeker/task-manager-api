namespace todoApp.Core.Interfaces;

public interface IEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent;
    
}