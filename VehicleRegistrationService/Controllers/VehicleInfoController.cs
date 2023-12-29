namespace VehicleRegistrationService.Controllers;

[ApiController]
[Route("[controller]")]
public class VehicleInfoController(ILogger<VehicleInfoController> logger, IVehicleInfoRepository vehicleInfoRepository)
    : ControllerBase
{
    [HttpGet("{licenseNumber}")]
    public ActionResult<VehicleInfo> GetVehicleInfo(string licenseNumber)
    {
        logger.LogInformation("Retrieving vehicle-info for license number {LicenseNumber}", licenseNumber);
        var info = vehicleInfoRepository.GetVehicleInfo(licenseNumber);
        return info;
    }
}