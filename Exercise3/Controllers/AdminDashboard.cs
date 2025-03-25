using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Exercise1.Controllers;

[ApiController]
public class AdminDashboard(IMqttPublisher mqttPublisher) : ControllerBase
{

    [HttpGet]
    [Route("preferences")]
    public ActionResult AdminChangesDevicePreferences([FromQuery] string? deviceId = "A", [FromQuery]string? preference ="go faster")
    {
        mqttPublisher.Publish("device/" + deviceId + "/preferences", 
            JsonSerializer.Serialize(new {preferences = preference}));
        return Ok("you have now changed the preferences for device");
    }
}