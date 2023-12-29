namespace FineCollectionService.Proxies;

public class VehicleRegistrationService(HttpClient httpClient)
{
    public async Task<VehicleInfo> GetVehicleInfo(string licenseNumber)
    {
        // Because the HttpClient passed into this class has already been created for a certain app-id, you can omit
        // the host information from the request URL. 
        return await httpClient.GetFromJsonAsync<VehicleInfo>(
            $"/vehicleinfo/{licenseNumber}");
    }
}
