using GateWay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GateWay.Helper_Classes
{
    public static class ModbusRegisterLoader
    {
        public static List<ModbusRegister> LoadRegisters(string filePath, ILogger logger)
        {
            try
            {
                logger.LogInformation("Loading registers from {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    logger.LogError("Register file not found at path: {FilePath}", filePath);
                    return new List<ModbusRegister>();
                }

                var json = File.ReadAllText(filePath);
                var registers = JsonSerializer.Deserialize<List<ModbusRegister>>(json);

                logger.LogInformation("Loaded {Count} registers", registers?.Count ?? 0);
                return registers ?? new List<ModbusRegister>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading registers from file: {FilePath}", filePath);
                return new List<ModbusRegister>();
            }
        }
    }
}
