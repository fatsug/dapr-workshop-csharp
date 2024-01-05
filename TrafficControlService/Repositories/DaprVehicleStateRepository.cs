namespace TrafficControlService.Repositories;

public class DaprVehicleStateRepository(DaprClient daprClient) : IVehicleStateRepository
{
    private const string DAPR_STORE_NAME = "statestore";

    public async Task SaveVehicleStateAsync(VehicleState vehicleState)
    {
        await daprClient.SaveStateAsync(DAPR_STORE_NAME, vehicleState.LicenseNumber, vehicleState);
    }

    public async Task<VehicleState?> GetVehicleStateAsync(string licenseNumber)
    {
        return await daprClient.GetStateAsync<VehicleState>(DAPR_STORE_NAME, licenseNumber);
    }
}