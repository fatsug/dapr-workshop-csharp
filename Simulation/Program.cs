const int lanes = 3;
var cameras = new CameraSimulation[lanes];
var httpClient = new HttpClient();

for (var i = 0; i < lanes; i++)
{
    var camNumber = i + 1;
    var trafficControlService = new HttpTrafficControlService(httpClient);
    cameras[i] = new CameraSimulation(camNumber, trafficControlService);
}
Parallel.ForEach(cameras, cam => cam.Start());

Task.Run(() => Thread.Sleep(Timeout.Infinite)).Wait();