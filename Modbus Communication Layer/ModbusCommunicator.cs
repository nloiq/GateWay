using GateWay.Helper_Classes;
using GateWay.Models;
using Microsoft.Extensions.Options;
using NModbus;
using NModbus.IO;
using NModbus.Serial;
using System.Diagnostics;
using System.IO.Ports;
using System.Net.Sockets;

namespace GateWay.Modbus_Communication_Layer
{
    public class ModbusCommunicator : IModbusCommunicator
    {
        private readonly ILogger<ModbusCommunicator> _logger;
        private readonly ModbusSettings _modbusSettings;
        private readonly ModBusProtocolSettings _modBusProtocol;
        private IModbusMaster _master;
        private SerialPort _serialPort;

        public ModbusCommunicator(
            ILogger<ModbusCommunicator> logger,
            IOptions<ModbusSettings> modbusOptions,
            IOptions<ModBusProtocolSettings> modbusProtocol)
        {
            _logger = logger;
            _modbusSettings = modbusOptions.Value;
            _modBusProtocol = modbusProtocol.Value;
            _master = null;
            _serialPort = null;
        }
        public async Task<List<RealTimeData>> ReadRegistersAsync(List<ModbusRegister> registers, byte unitId, CancellationToken token)
{
    // Validate input parameters
    if (registers == null || registers.Count == 0)
    {
        _logger.LogWarning("Empty or null registers list provided");
        return new List<RealTimeData>();
    }

    try
    {
        // Check and establish connection if needed
        if (!await IsConnectionValidAsync())
        {
            await ReconnectAsync(token);
        }

        // Perform the actual register reading
        return await ReadModbusRegistersAsync(_master, registers, unitId, token);
    }
    catch (SlaveException  mbEx)
    {
        _logger.LogError(mbEx, "Modbus protocol error occurred");
        DisposeMaster(); // Clean up broken connection
        throw;
    }
    catch (IOException ioEx)
    {
        _logger.LogError(ioEx, "I/O error during Modbus communication");
        DisposeMaster();
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error reading registers");
        throw;
    }
}

private async Task<bool> IsConnectionValidAsync()
{
    if (_master == null) return false;

    if (_modBusProtocol.Type == "RS485")
    {
        return _serialPort != null && _serialPort.IsOpen;
    }
    else // TCP
    {
        try
        {
            // Safer way to check TCP connection without reflection
            if (_master.Transport is ModbusIpTransport ipTransport)
            {
                return ipTransport.IsSocketConnected();
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}

private async Task ReconnectAsync(CancellationToken token)
{
    int attempt = 0;
    const int maxAttempts = 3;

    while (attempt < maxAttempts && !token.IsCancellationRequested)
    {
        try
        {
            DisposeMaster(); // Clean up previous connection
            await InitializeModbusMasterAsync(token);
            return;
        }
        catch (Exception ex)
        {
            attempt++;
            _logger.LogWarning(ex, "Reconnection attempt {Attempt}/{MaxAttempts} failed", 
                attempt, maxAttempts);

            if (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), token);
            }
        }
    }

    throw new InvalidOperationException($"Failed to establish Modbus connection after {maxAttempts} attempts");
}

private void DisposeMaster()
{
    try
    {
        if (_master != null)
        {
            _master.Dispose();
            _master = null;
            _logger.LogDebug("Modbus master disposed successfully");
        }

        if (_modBusProtocol.Type == "RS485" && _serialPort != null)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _logger.LogDebug("Serial port closed successfully");
                }
                _serialPort.Dispose();
                _serialPort = null;
            }
            catch (IOException ioEx)
            {
                _logger.LogWarning(ioEx, "Error closing serial port");
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Error during Modbus master cleanup");
    }
    finally
    {
        _master = null;
        _serialPort = null;
    }
}
        //public async Task<List<RealTimeData>> ReadRegistersAsync(List<ModbusRegister> registers, byte unitId, CancellationToken token)
        //{
        //    if (_master == null || (_modBusProtocol.Type == "TCP" && !((TcpClient)_master.Transport.GetType().GetField("_client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_master.Transport)).Connected))
        //    {
        //        await InitializeModbusMasterAsync(token);
        //    }

        //    return await ReadModbusRegistersAsync(_master, registers, unitId, token);
        //}
        //private void DisposeMaster()
        //{
        //    try
        //    {
        //        _master?.Dispose();
        //        _master = null;

        //        if (_modBusProtocol.Type == "RS485")
        //        {
        //            _serialPort?.Close();
        //            _serialPort?.Dispose();
        //            _serialPort = null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogWarning(ex, "Error during Modbus master cleanup");
        //    }
        //}
        private async Task InitializeModbusMasterAsync(CancellationToken token)
        {
            DisposeMaster(); // Clean up existing connection

            var factory = new ModbusFactory();

            if (_modBusProtocol.Type == "RS485")
            {
                _serialPort = CreateSerialPort();
                await InitializeSerialPortAsync(_serialPort, token);
                _master = factory.CreateRtuMaster(_serialPort);
            }
            else // TCP
            {
                var client = new TcpClient();
                await client.ConnectAsync(_modbusSettings.Host, _modbusSettings.Port, token);
                _master = factory.CreateMaster(client);
            }

            ConfigureModbusMaster(_master);
        }

        private void ConfigureModbusMaster(IModbusMaster master)
        {
            master.Transport.Retries = _modbusSettings.Retries;
            master.Transport.WaitToRetryMilliseconds = _modbusSettings.WaitToRetryMilliseconds;

            if (_modBusProtocol.Type == "TCP")
            {
                master.Transport.ReadTimeout = _modbusSettings.ReadTimeout;
                master.Transport.WriteTimeout = _modbusSettings.WriteTimeout;
            }
        }

        private SerialPort CreateSerialPort()
        {
            return new SerialPort(_modbusSettings.SerialPortName)
            {
                BaudRate = _modbusSettings.BaudRate,
                DataBits = _modbusSettings.DataBits,
                Parity = SerialPortHelper.GetParity(_modbusSettings.Parity),
                StopBits = SerialPortHelper.GetStopBits(_modbusSettings.StopBits),
                ReadTimeout = _modbusSettings.ReadTimeout,
                WriteTimeout = _modbusSettings.WriteTimeout,
                Handshake = Handshake.None
            };
        }

        private async Task InitializeSerialPortAsync(SerialPort serialPort, CancellationToken token)
        {
            int attempt = 0;
            const int maxAttempts = 3;

            while (attempt < maxAttempts && !token.IsCancellationRequested)
            {
                try
                {
                    serialPort.Open();
                    return;
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                {
                    attempt++;
                    _logger.LogWarning(ex, "Failed to open serial port (attempt {Attempt}/{MaxAttempts})",
                        attempt, maxAttempts);

                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(1000 * attempt, token);
                    }
                }
            }

            throw new InvalidOperationException($"Failed to open serial port after {maxAttempts} attempts");
        }

        private async Task<List<RealTimeData>> ReadModbusRegistersAsync(
            IModbusMaster master,
            List<ModbusRegister> registers,
            byte unitId,
            CancellationToken token)
        {
            var dataList = new List<RealTimeData>();
            var stopwatch = Stopwatch.StartNew();

            foreach (var reg in registers)
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    int value = reg.Type switch
                    {
                        "SPI" or "DPI" => await ReadCoilWithTimeout(master, unitId, reg.Address, token),
                        "AMI" => await ReadHoldingRegisterWithTimeout(master, unitId, reg.Address, token),
                        _ => throw new InvalidOperationException($"Unknown register type: {reg.Type}")
                    };

                    dataList.Add(new RealTimeData
                    {
                        Address = reg.Address,
                        Value = value,
                        Type = reg.Type,
                    });
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Register reading was cancelled");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read register at address {Address}", reg.Address);
                }
            }

            stopwatch.Stop();
            _logger.LogDebug("Register read completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

            return dataList;
        }

        private async Task<int> ReadCoilWithTimeout(IModbusMaster master, byte unitId, int address, CancellationToken token)
        {
            try
            {
                var readTask = Task.Run(() =>
                {
                    var result = master.ReadCoils(unitId, (ushort)address, 1);
                    return result[0] ? 1 : 0;
                }, token);

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(_modbusSettings.ReadTimeout / 1000.0), token);
                var completedTask = await Task.WhenAny(readTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"Read coil at address {address} timed out");
                }

                return await readTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading coil at address {Address}", address);
                throw;
            }
        }

        private async Task<int> ReadHoldingRegisterWithTimeout(IModbusMaster master, byte unitId, int address, CancellationToken token)
        {
            try
            {
                var readTask = Task.Run(() =>
                {
                    var result = master.ReadHoldingRegisters(unitId, (ushort)address, 1);
                    return (int)result[0];
                }, token);

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(_modbusSettings.ReadTimeout / 1000.0), token);
                var completedTask = await Task.WhenAny(readTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"Read holding register at address {address} timed out");
                }

                return await readTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading holding register at address {Address}", address);
                throw;
            }
        }

        public void Dispose()
        {
            _master?.Dispose();
            _serialPort?.Close();
            _serialPort?.Dispose();
        }
    }
}
