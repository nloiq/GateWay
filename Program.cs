using GateWay.API_Communication_Layer;
using GateWay.Data_Logging_Layer;
using GateWay.Data_Processing_Layer;
using GateWay.Modbus_Communication_Layer;
using GateWay.Models;
using GateWay;

var builder = Host.CreateApplicationBuilder(args);

// Add this line before your other service registrations
builder.Services.AddHttpClient();  // This registers IHttpClientFactory

// Your existing registrations
builder.Services.AddSingleton<IModbusCommunicator, ModbusCommunicator>();
builder.Services.AddSingleton<IDataProcessor, DataProcessor>();
builder.Services.AddSingleton<IApiClient, ApiClient>();
builder.Services.AddSingleton<IDataLogger, JsonFileLogger>();

// Configuration
builder.Services.Configure<ModbusSettings>(builder.Configuration.GetSection("ModbusSettings"));
builder.Services.Configure<ModBusProtocolSettings>(builder.Configuration.GetSection("ModBusProtocolSettings"));
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();