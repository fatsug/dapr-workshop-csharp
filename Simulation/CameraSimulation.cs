namespace Simulation;

public class CameraSimulation(int camNumber, ITrafficControlService trafficControlService)
{
    private readonly Random _rnd = new();
    private const int MinEntryDelayInMs = 50;
    private const int MaxEntryDelayInMs = 5000;
    private const int MinExitDelayInS = 4;
    private const int MaxExitDelayInS = 10;

    public Task Start()
    {
        Console.WriteLine($"Start camera {camNumber} simulation.");

        while (true)
        {
            var entryDelay = TimeSpan.FromMilliseconds(_rnd.Next(MinEntryDelayInMs, MaxEntryDelayInMs) + _rnd.NextDouble());
            try
            {
                // simulate entry
                Task.Delay(entryDelay).Wait();
                var entryTimestamp = DateTime.Now;
                
                Task.Run(async () =>
                {
                    // simulate entry
                    var vehicleRegistered = new VehicleRegistered
                    {
                        Lane = camNumber,
                        LicenseNumber = GenerateRandomLicenseNumber(),
                        Timestamp = entryTimestamp
                    };
                    await trafficControlService.SendVehicleEntryAsync(vehicleRegistered);
                    Console.WriteLine($"Simulated ENTRY of vehicle with license-number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");


                    // simulate exit
                    var exitDelay = TimeSpan.FromSeconds(_rnd.Next(MinExitDelayInS, MaxExitDelayInS) + _rnd.NextDouble());
                    Task.Delay(exitDelay).Wait();
                    vehicleRegistered.Timestamp = DateTime.Now;
                    vehicleRegistered.Lane = _rnd.Next(1, 4);
                    await trafficControlService.SendVehicleExitAsync(vehicleRegistered);
                    Console.WriteLine($"Simulated EXIT of vehicle with license-number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");
                }).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Camera {camNumber} error: {ex.Message}");
            }
        }
        
        // ReSharper disable once FunctionNeverReturns
    }

    #region Private helper methods

    private const string ValidLicenseNumberChars = "DFGHJKLNPRSTXYZ";

    private string GenerateRandomLicenseNumber()
    {
        var type = _rnd.Next(1, 9);
        
        var kenteken = type switch
        {
            1 => // 99-AA-99
                $"{_rnd.Next(1, 99):00}-{GenerateRandomCharacters(2)}-{_rnd.Next(1, 99):00}",
            2 => // AA-99-AA
                $"{GenerateRandomCharacters(2)}-{_rnd.Next(1, 99):00}-{GenerateRandomCharacters(2)}",
            3 => // AA-AA-99
                $"{GenerateRandomCharacters(2)}-{GenerateRandomCharacters(2)}-{_rnd.Next(1, 99):00}",
            4 => // 99-AA-AA
                $"{_rnd.Next(1, 99):00}-{GenerateRandomCharacters(2)}-{GenerateRandomCharacters(2)}",
            5 => // 99-AAA-9
                $"{_rnd.Next(1, 99):00}-{GenerateRandomCharacters(3)}-{_rnd.Next(1, 10)}",
            6 => // 9-AAA-99
                $"{_rnd.Next(1, 9)}-{GenerateRandomCharacters(3)}-{_rnd.Next(1, 10):00}",
            7 => // AA-999-A
                $"{GenerateRandomCharacters(2)}-{_rnd.Next(1, 999):000}-{GenerateRandomCharacters(1)}",
            8 => // A-999-AA
                $"{GenerateRandomCharacters(1)}-{_rnd.Next(1, 999):000}-{GenerateRandomCharacters(2)}",
            _ => string.Empty
        };

        return kenteken;
    }

    private string GenerateRandomCharacters(int aantal)
    {
        var chars = new char[aantal];
        
        for (var i = 0; i < aantal; i++)
        {
            chars[i] = ValidLicenseNumberChars[_rnd.Next(ValidLicenseNumberChars.Length - 1)];
        }
        return new string(chars);
    }

    #endregion
}
