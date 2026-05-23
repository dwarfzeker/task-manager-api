namespace todoApp.Core.Interfaces;

public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event);
    public DateTime OccuredAt { get; set; }
}