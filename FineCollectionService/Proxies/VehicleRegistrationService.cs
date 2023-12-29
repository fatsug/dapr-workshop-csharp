namespace FineCollectionService.Proxies;

public class VehicleRegistrationService(HttpClient httpClient)
{
    public async Task<VehicleInfo> GetVehicleInfo(string licenseNumber)
    {
        // Invoke VehicleRegistrationService via FineCollectionService's own Dapr sidecar 
        return await httpClient.GetFromJsonAsync<VehicleInfo>(
            $"http://localhost:3601/v1.0/invoke/vehicleregistrationservice/method/vehicleinfo/{licenseNumber}");
    }
}
