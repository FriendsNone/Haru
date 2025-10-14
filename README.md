# Haru

A clean, modern weather application for Windows Phone that brings beautiful weather forecasts to your device.

![Windows Phone 7](https://img.shields.io/badge/Windows%20Phone-7.5+-blue.svg)
![.NET](https://img.shields.io/badge/.NET-Framework%204.0-512BD4.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

## Features

### Comprehensive Weather Data
- **Current conditions** with temperature, humidity, precipitation, wind, and pressure
- **Hourly forecasts** for the next 12 hours
- **Daily forecasts** for the next 7 days
- Beautiful weather icons with day/night variants
- Detailed weather descriptions and interpretations

### Smart Location Search
- Search locations worldwide
- Detailed location information with administrative regions
- Easy location management

### Live Tile Integration
- Auto-updating live tile with current weather
- Temperature and conditions displayed on tile back
- Background updates every 30 minutes (when connected to Wi-Fi)
- Smart offline caching - shows last known weather when network unavailable
- Battery-efficient with network availability checks

### Customizable Settings
- **Temperature units**: Celsius (°C) or Fahrenheit (°F)
- **Wind speed units**: km/h, m/s, mph, or knots
- **Precipitation units**: mm or inches
- Toggle background tile updates
- Clear cached forecast data

### Offline Support
- Automatic weather data caching
- Displays last known conditions when offline
- Seamless online/offline transitions

## Screenshots

*Coming soon*

## Installation

### Option 1: From XAP File (Sideloading)
1. Download the latest `.xap` file from the [Releases](../../releases) page
2. Connect your Windows Phone 7 device to your computer
3. Use the **Windows Phone Application Deployment Tool** to deploy the XAP
4. Launch Haru from your app list

### Option 2: From Source
See [Build Instructions](#build-instructions) below

## Build Instructions

### Prerequisites

- **Windows 7** or higher
- **Zune software** (for Windows Phone 7 device deployment)
- **Git** (for cloning the repository)
- **NuGet** (for managing dependencies)
- **Visual Studio 2012** 
- **Windows Phone SDK 7.1**

### Required NuGet Packages

The project uses the following NuGet packages:

- `WPtoolkit` (4.2013.08.16)
- `Newtonsoft.Json` (4.5.11)
- `NodaTime` (1.2.0)

### Build Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/FriendsNone/Haru.git
   cd Haru
   ```

2. **Open the solution**
   - Open `Haru.sln` in Visual Studio

3. **Restore NuGet packages**
   - Right-click on the solution in Solution Explorer
   - Select "Restore NuGet Packages"
   - Or use Package Manager Console:
     ```
     Update-Package -Reinstall
     ```

4. **Set HaruApp as startup project**
   - Right-click on `HaruApp` project
   - Select "Set as StartUp Project"

5. **Build the solution**
   - Press `F6` or select **Build > Build Solution**
   - Ensure `HaruCore`, `HaruApp`, and `HaruAgent` projects build successfully

6. **Deploy to device or emulator**
   - Press `F5` to run with debugging
   - Or `Ctrl+F5` to run without debugging
   - Select Windows Phone Emulator or connected device

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

### Development Guidelines

- Follow existing code style and patterns
- Use LINQ for collection operations
- Keep methods concise and focused
- Add comments for complex logic
- Test on actual Windows Phone 7 device when possible

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Open-Meteo](https://open-meteo.com) for weather and geocoding APIs
- [basmilius](https://github.com/basmilius/weather-icons) for weather icons
- [Fluent System Icons](https://github.com/microsoft/fluentui-system-icons) for Haru app icon
- Built for the Windows Phone community

## Support

Having issues? Here are some resources:
- Check the [Issues](../../issues) page for known problems