# GateWay Modbus Communication Service

GateWay is a .NET BackgroundService that continuously reads data from Modbus-compatible devices using either RS485 or TCP protocols, processes the data, and optionally sends it to an API endpoint. This modular and configurable architecture makes it suitable for industrial or energy applications that rely on Modbus data acquisition.

## ?? Features

* ?? Supports both **Modbus RTU (RS485)** and **Modbus TCP**
* ?? Configurable register mappings via JSON
* ?? Automatic polling at configurable intervals
* ?? Reconnection handling for unstable links
* ?? Decoupled data processing logic
* ?? Easily extendable for cloud integration or local storage

---

## ?? Getting Started

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
* Compatible with Windows and Linux
* Access to Modbus-compatible device (RTU or TCP)

---

## ?? Installation & Configuration

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/GateWay.git
cd GateWay
```

### 2. Configure `appsettings.json`

Update the following sections based on your protocol:

#### ? ModbusSettings

```json
"ModbusSettings": {
  "RegisterFile": "config/registers.json",    // Path to register definitions
  "UnitId": 1,                                 // Modbus slave ID
  "PollingIntervalSeconds": 5,                 // Polling frequency

  // RS485 Settings (Used only if Type is "RS485")
  "SerialPortName": "COM10",
  "BaudRate": 9600,
  "DataBits": 8,
  "Parity": "None",                            // None, Odd, Even
  "StopBits": "One",                           // One, Two
  "ReadTimeout": 1000,
  "WriteTimeout": 1000,

  // TCP Settings (Used only if Type is "TCP")
  "Host": "127.0.0.1",
  "Port": 502,

  "Retries": 3,
  "WaitToRetryMilliseconds": 1000
}
```

#### ? ModBusProtocolSettings

```json
"ModBusProtocolSettings": {
  "Type": "RS485",                // "RS485" or "TCP"
  "EnableAutoReconnect": true,
  "MaxReconnectAttempts": 5
}
```

#### ? Optional: ApiSettings (for data upload)

```json
"ApiSettings": {
  "BaseUrl": "https://anbar.nloiq.com/api/GateWay",
  "TimeoutSeconds": 30
}
```

---

## ?? Register File Format (`config/registers.json`)

This file contains register mappings. Example format:

```json
[
  {
    "Name": "Voltage",
    "Address": 0,
    "Length": 2,
    "DataType": "Float"
  },
  {
    "Name": "Current",
    "Address": 2,
    "Length": 2,
    "DataType": "Float"
  }
]
```

---

## ????? Running the Service

### 1. Build and Run

```bash
dotnet build
dotnet run
```

### 2. As a Background Service (Optional)

For deployment, it can be hosted as a systemd service (Linux) or Windows Service.

---

## ??? Architecture Overview

```
???????????????????????????????????????????????
?         Worker.cs              ?
? Reads Modbus and calls        ?
? IDataProcessor                ?
?????????????????????????????
             ?
   ???????????????????????????????
   ? IModbusCommunicator ?  <?? [RS485 or TCP Implementation]
   ?????????????????????????????
             ?
   ?????????????????????????????
   ? Register Loader      ?  <?? Reads register file (JSON)
   ?????????????????????????????
             ?
   ?????????????????????????????
   ? IDataProcessor      ?  <?? Custom data handling / API call
   ?????????????????????????????
```

---

## ?? Publish

To publish for deployment:

```bash
dotnet publish -c Release -o ./publish
```

Deploy the contents of the `./publish` folder to your target environment.

---

## ?? License

This project is licensed under the MIT License.

---

## ?? Contributing

Contributions are welcome! Feel free to fork the repo and submit a pull request.

---

## ?? Tips

* Use [NModbus](https://github.com/NModbus/NModbus) or custom implementations for communication.
* Register definitions must match your device documentation.
* Ensure serial port permissions on Linux using:

  ```bash
  sudo usermod -a -G dialout $USER
  ```

---

## ?? Contact

For questions or feature requests, open an issue or contact \[[nlobaghdad@gmail.com](mailto:youremail@example.com)].
