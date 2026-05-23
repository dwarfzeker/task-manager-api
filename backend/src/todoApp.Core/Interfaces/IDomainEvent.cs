namespace todoApp.Core.Interfaces;

public interface IDomainEvent
{
    public DateTime OccuredAt { get; set; }
}