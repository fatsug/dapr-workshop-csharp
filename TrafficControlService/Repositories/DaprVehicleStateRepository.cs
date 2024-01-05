namespace TrafficControlService.Repositories;

public class DaprVehicleStateRepository(HttpClient httpClient) : IVehicleStateRepository
{
    private const string DAPR_STORE_NAME = "statestore";

    public async Task SaveVehicleStateAsync(VehicleState vehicleState)
    {
        var state = new[]
        {
            new 
            { 
                key = vehicleState.LicenseNumber,
                value = vehicleState
            }
        };

        await httpClient.PostAsJsonAsync(
            $"http://localhost:3600/v1.0/state/{DAPR_STORE_NAME}",
            state);
    }

    public async Task<VehicleState?> GetVehicleStateAsync(string licenseNumber)
    {
        var state = await httpClient.GetFromJsonAsync<VehicleState>(
            $"http://localhost:3600/v1.0/state/{DAPR_STORE_NAME}/{licenseNumber}");
        
        return state;
    }
}