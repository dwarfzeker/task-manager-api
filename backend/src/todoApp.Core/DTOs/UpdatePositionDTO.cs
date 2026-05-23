namespace todoApp.Core.DTOs;

public class UpdatePositionDTO
{
    public Guid Id { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
}