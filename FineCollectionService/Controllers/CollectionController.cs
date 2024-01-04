﻿namespace FineCollectionService.Controllers;

[ApiController]
[Route("")]
public class CollectionController : ControllerBase
{
    private static string? _fineCalculatorLicenseKey = null;
    private readonly ILogger<CollectionController> _logger;
    private readonly IFineCalculator _fineCalculator;
    private readonly VehicleRegistrationService _vehicleRegistrationService;

    public CollectionController(IConfiguration config, ILogger<CollectionController> logger,
        IFineCalculator fineCalculator, VehicleRegistrationService vehicleRegistrationService)
    {
        _logger = logger;
        _fineCalculator = fineCalculator;
        _vehicleRegistrationService = vehicleRegistrationService;

        // set finecalculator component license-key
        _fineCalculatorLicenseKey ??= config.GetValue<string>("fineCalculatorLicenseKey");
    }

    [Route("collectfine")]
    [HttpPost()]
    public async Task<ActionResult> CollectFine([FromBody] System.Text.Json.JsonDocument cloudevent)
    {
        var data = cloudevent.RootElement.GetProperty("data");
        var speedingViolation = new SpeedingViolation
        {
            VehicleId = data.GetProperty("vehicleId").GetString()!,
            RoadId = data.GetProperty("roadId").GetString()!,
            Timestamp = data.GetProperty("timestamp").GetDateTime()!,
            ViolationInKmh = data.GetProperty("violationInKmh").GetInt32()
        };
        
        decimal fine = _fineCalculator.CalculateFine(_fineCalculatorLicenseKey!, speedingViolation.ViolationInKmh);

        // get owner info
        var vehicleInfo = await _vehicleRegistrationService.GetVehicleInfo(speedingViolation.VehicleId);

        // log fine
        var fineString = fine == 0 ? "tbd by the prosecutor" : $"{fine} Euro";
        
        _logger.LogInformation("Sent speeding ticket to {OwnerName}. " +
            "Road: {RoadId}, License Number: {VehicleId}, Vehicle: {Brand} {Model}, " +
            "Violation: {ViolationInKmh} Km/h, Fine: {FineString}, " +
            "On: {Date} at {Time}", 
            vehicleInfo.OwnerName, 
            speedingViolation.RoadId, 
            speedingViolation.VehicleId,
            vehicleInfo.Brand, 
            vehicleInfo.Model, 
            speedingViolation.ViolationInKmh, 
            fineString, 
            speedingViolation.Timestamp.ToString("dd-MM-yyyy"),
            speedingViolation.Timestamp.ToString("hh:mm:ss"));

        // send fine by email
        // TODO

        return Ok();
    }
    
    [Route("/dapr/subscribe")]
    [HttpGet()]
    public object Subscribe()
    {
        return new object[]
        {
            new
            {
                pubsubname = "pubsub",
                topic = "speedingviolations",
                route = "/collectfine"
            }
        };
    }
}