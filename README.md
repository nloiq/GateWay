# Modbus Gateway Service

A background service that reads data from Modbus devices (RS485 or TCP) and processes it for further use.

## Features

- Supports both Modbus RTU (RS485) and Modbus TCP protocols
- Configurable polling interval
- Automatic reconnection support
- Flexible register configuration via JSON
- Data processing pipeline
- Logging support

## Prerequisites

- .NET 6.0 or later
- For RS485 communication:
  - Compatible serial port
  - Appropriate USB-to-RS485 converter if needed

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/modbus-gateway.git
   cd modbus-gateway
