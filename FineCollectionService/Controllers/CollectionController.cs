﻿using Dapr;

namespace FineCollectionService.Controllers;

[ApiController]
[Route("")]
public class CollectionController : ControllerBase
{
    private static string? _fineCalculatorLicenseKey = null;
    private readonly ILogger<CollectionController> _logger;
    private readonly IFineCalculator _fineCalculator;
    private readonly VehicleRegistrationService _vehicleRegistrationService;

    public CollectionController(IConfiguration config, ILogger<CollectionController> logger,
        IFineCalculator fineCalculator, VehicleRegistrationService vehicleRegistrationService, DaprClient daprClient)
    {
        _logger = logger;
        _fineCalculator = fineCalculator;
        _vehicleRegistrationService = vehicleRegistrationService;

        // set finecalculator component license-key
        var secrets = daprClient.GetSecretAsync(
            "trafficcontrol-secrets", "finecalculator.licensekey").Result;
        _fineCalculatorLicenseKey = secrets["finecalculator.licensekey"];
    }

    [Topic("pubsub", "speedingviolations")]
    [Route("collectfine")]
    [HttpPost()]
    public async Task<ActionResult> CollectFine(SpeedingViolation speedingViolation, [FromServices] DaprClient daprClient)
    {
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
        var body = EmailUtils.CreateEmailBody(speedingViolation, vehicleInfo, fineString);

        var metadata = new Dictionary<string, string>
        {
            ["emailFrom"] = "noreply@cfca.gov",
            ["emailTo"] = vehicleInfo.OwnerEmail,
            ["subject"] = $"Speeding violation on the {speedingViolation.RoadId}"
        };
        
        await daprClient.InvokeBindingAsync("sendmail", "create", body, metadata);
        
        return Ok();
    }
}