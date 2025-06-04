using GateWay.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GateWay.Data_Logging_Layer
{
    public class JsonFileLogger : IDataLogger
    {
        private readonly ILogger<JsonFileLogger> _logger;
        private readonly string _logFilePath;
        private readonly long _maxFileSize = 1024 * 1024; // 1MB
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false // Compact JSON format
        };

        public JsonFileLogger(ILogger<JsonFileLogger> logger, string logFilePath = "data_log.jsonl")
        {
            _logger = logger;
            _logFilePath = logFilePath;
        }

        public async Task AppendAsync(List<RealTimeData> data, CancellationToken token)
        {
            if (data == null || data.Count == 0)
            {
                return;
            }

            await _fileLock.WaitAsync(token);
            try
            {
                // Check current file size
                bool fileExists = File.Exists(_logFilePath);
                long currentSize = fileExists ? new FileInfo(_logFilePath).Length : 0;

                // Rotate file if needed before writing new data
                if (fileExists && currentSize >= _maxFileSize)
                {
                    await RotateFile(token);
                }

                // Write new data line by line
                await using (var stream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                await using (var writer = new StreamWriter(stream))
                {
                    foreach (var item in data)
                    {
                        string jsonLine = JsonSerializer.Serialize(item, _jsonOptions);
                        await writer.WriteLineAsync(jsonLine);
                    }
                }

                _logger.LogInformation("Data logged locally: {Count} records", data.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging data to file");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task RotateFile(CancellationToken token)
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                // Read all lines from current file
                var lines = await File.ReadAllLinesAsync(_logFilePath, token);

                // Calculate how many lines to keep to stay under limit
                int linesToKeep = 0;
                long estimatedSize = 0;

                // Start from the end (newest data) and work backwards
                for (int i = lines.Length - 1; i >= 0; i--)
                {
                    estimatedSize += Encoding.UTF8.GetByteCount(lines[i]) + Environment.NewLine.Length;
                    if (estimatedSize >= _maxFileSize / 2) // Keep half capacity for new data
                    {
                        linesToKeep = lines.Length - i - 1;
                        break;
                    }
                }

                // Write the newest lines to temp file
                await File.WriteAllLinesAsync(tempFile, lines.Skip(lines.Length - linesToKeep), token);

                // Replace original file with the trimmed version
                File.Delete(_logFilePath);
                File.Move(tempFile, _logFilePath);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}