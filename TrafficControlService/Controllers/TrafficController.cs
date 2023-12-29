namespace TrafficControlService.Controllers;

[ApiController]
[Route("")]
public class TrafficController(
    ILogger<TrafficController> logger,
    HttpClient httpClient,
    IVehicleStateRepository vehicleStateRepository,
    ISpeedingViolationCalculator speedingViolationCalculator)
    : ControllerBase
{
    private readonly string _roadId = speedingViolationCalculator.GetRoadId();

    [HttpPost("entrycam")]
    public async Task<ActionResult> VehicleEntry(VehicleRegistered msg)
    {
        try
        {
            // log entry
            logger.LogInformation(
                "ENTRY detected in lane {Lane} at {Time} of vehicle with license-number {LicenseNumber}",
                msg.Lane,
                msg.Timestamp.ToString("hh:mm:ss"),
                msg.LicenseNumber);

            // store vehicle state
            var vehicleState = new VehicleState
            {
                LicenseNumber = msg.LicenseNumber,
                EntryTimestamp = msg.Timestamp
            };
            await vehicleStateRepository.SaveVehicleStateAsync(vehicleState);

            return Ok();
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("exitcam")]
    public async Task<ActionResult> VehicleExit(VehicleRegistered msg)
    {
        try
        {
            // get vehicle state
            var vehicleState = await vehicleStateRepository.GetVehicleStateAsync(msg.LicenseNumber);
            if (!vehicleState.HasValue)
            {
                return NotFound();
            }

            // log exit
            logger.LogInformation(
                "EXIT detected in lane {Lane} at {Time} of vehicle with license-number {LicenseNumber}",
                msg.Lane,
                msg.Timestamp.ToString("hh:mm:ss"),
                msg.LicenseNumber);

            // update state
            vehicleState = vehicleState.Value with {ExitTimestamp = msg.Timestamp};
            await vehicleStateRepository.SaveVehicleStateAsync(vehicleState.Value);

            // handle possible speeding violation
            var violation = speedingViolationCalculator.DetermineSpeedingViolationInKmh(
                vehicleState.Value.EntryTimestamp, vehicleState.Value.ExitTimestamp.Value);
            
            if (violation <= 0) return Ok();
            
            logger.LogInformation(
                "Speeding violation detected ({Violation} KMh) of vehicle with license-number {LicenseNumber}", 
                violation, 
                vehicleState.Value.LicenseNumber);

            var speedingViolation = new SpeedingViolation
            {
                VehicleId = msg.LicenseNumber,
                RoadId = _roadId,
                ViolationInKmh = violation,
                Timestamp = msg.Timestamp
            };

            // publish speedingviolation
            var message = JsonContent.Create<SpeedingViolation>(speedingViolation);
            await httpClient.PostAsync("http://localhost:3600/v1.0/publish/pubsub/speedingviolations", message);

            return Ok();
        }
        catch
        {
            return StatusCode(500);
        }
    }
}