namespace TrafficControlService.Repositories;

public class InMemoryVehicleStateRepository : IVehicleStateRepository
{
    private readonly ConcurrentDictionary<string, VehicleState> _state = new();

    public Task<VehicleState?> GetVehicleStateAsync(string licenseNumber)
    {
        return !_state.TryGetValue(licenseNumber, out var vehicleState) 
            ? Task.FromResult<VehicleState?>(null) 
            : Task.FromResult<VehicleState?>(vehicleState);
    }

    public Task SaveVehicleStateAsync(VehicleState vehicleState)
    {
        _state.AddOrUpdate(vehicleState.LicenseNumber, vehicleState,
            (k, s) => vehicleState);
        return Task.CompletedTask;
    }
}
