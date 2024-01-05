// create web-app
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IFineCalculator, HardCodedFineCalculator>();

builder.Services.AddDaprClient(clientBuilder => clientBuilder
    .UseHttpEndpoint($"http://localhost:3601")
    .UseGrpcEndpoint($"http://localhost:60001"));

builder.Services.AddSingleton<VehicleRegistrationService>(_ => 
    new VehicleRegistrationService(DaprClient.CreateInvokeHttpClient(
        "vehicleregistrationservice", "http://localhost:3601")));

builder.Services.AddControllers().AddDapr();

var app = builder.Build();

// configure web-app
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// configure routing
app.MapControllers();

// configure Dapr
app.UseCloudEvents();
app.MapSubscribeHandler();

// let's go!
app.Run("http://localhost:6001");
