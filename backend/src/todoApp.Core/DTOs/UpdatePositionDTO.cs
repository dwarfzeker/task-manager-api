using System.Diagnostics.CodeAnalysis;
namespace todoApp.Core.DTOs;

[ExcludeFromCodeCoverage]
public class UpdatePositionDTO
{
    public Guid Id { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
}