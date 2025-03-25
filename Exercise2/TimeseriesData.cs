using System.ComponentModel.DataAnnotations;

public class TimeseriesData {
    [Key]
    public string? Id { get; set; }
    public string DeviceId { get; set; } = null!;
    public string Data { get; set; } = null!;
}