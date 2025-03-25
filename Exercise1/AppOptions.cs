using System.ComponentModel.DataAnnotations;

public class AppOptions
{
    [Required]
    public string MQTT_BROKER_HOST { get; set; }
    [Required]
    public string MQTT_USERNAME { get; set; }
    [Required]
    public string MQTT_PASSWORD { get; set; }
}