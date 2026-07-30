using System.ComponentModel.DataAnnotations;

namespace SpecCheck.GameData.Core.Models;

public class Game
{
    public int Id { get; set; }

    [MaxLength(500)]
    public required string Title { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }

    public int? ReleaseYear { get; set; }

    public double? IgdbRating { get; set; }

    [MaxLength(500)]
    public string? CoverUrl { get; set; }

    public string? Summary { get; set; }

    public ICollection<GameRequirement> Requirements { get; set; } = [];
    public ICollection<FPSEstimate> FpsEstimates { get; set; } = [];
}
