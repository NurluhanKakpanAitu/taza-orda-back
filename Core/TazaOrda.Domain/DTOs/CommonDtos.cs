namespace TazaOrda.Domain.DTOs;

/// <summary>
/// DTO для координат местоположения
/// </summary>
public record LocationDto
{
    public double Lat { get; init; }
    public double Lng { get; init; }
}
