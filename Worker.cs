using GateWay.Data_Processing_Layer;
using GateWay.Helper_Classes;
using GateWay.Modbus_Communication_Layer;
using GateWay.Models;
using Microsoft.Extensions.Options;

namespace GateWay
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IModbusCommunicator _modbusCommunicator;
        private readonly IDataProcessor _dataProcessor;
        private readonly ModbusSettings _modbusSettings;
        private readonly ModBusProtocolSettings _modBusProtocol;

        public Worker(
            ILogger<Worker> logger,
            IModbusCommunicator modbusCommunicator,
            IDataProcessor dataProcessor,
            IOptions<ModbusSettings> modbusOptions,
            IOptions<ModBusProtocolSettings> modbusProtocol)
        {
            _logger = logger;
            _modbusCommunicator = modbusCommunicator;
            _dataProcessor = dataProcessor;
            _modbusSettings = modbusOptions.Value;
            _modBusProtocol = modbusProtocol.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var registers = ModbusRegisterLoader.LoadRegisters(_modbusSettings.RegisterFile, _logger);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var dataList = await _modbusCommunicator.ReadRegistersAsync(registers, _modbusSettings.UnitId, stoppingToken);
                    _logger.LogInformation("Read {Count} registers from {Device}",
                        dataList.Count,
                        _modBusProtocol.Type == "RS485" ? _modbusSettings.SerialPortName : _modbusSettings.Host);

                    await _dataProcessor.ProcessDataAsync(dataList, stoppingToken);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    _logger.LogError(ex, "Modbus communication error");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during operation");
                }

                await Task.Delay(TimeSpan.FromSeconds(_modbusSettings.PollingIntervalSeconds), stoppingToken);
            }
        }
    }
}
