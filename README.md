# HotkeyAutomation
Trigger web services using a keyboard and raspberry pi.

## Installation instructions

### Prerequisites

I recommend you use a recent version of the Raspbian operating system, and have it configured to boot directly to the desktop without requiring authentication.

### Installation Script

Run these commands on your pi:

```
wget https://raw.githubusercontent.com/bp2008/HotkeyAutomation/master/HotkeyAutomation/HotkeyAutomation_Install.sh
chmod u+x HotkeyAutomation_Install.sh
./HotkeyAutomation_Install.sh
```

The installation script will ask if you wish to install or uninstall.  Once installed, HotkeyAutomation will start automatically upon booting to the desktop.

### Configuration

Navigate a web browser to the IP address of your raspberry pi.  The HotkeyAutomation program has a built-in web server which listens by default on port 80.

### Troubleshooting

This application is currently not set up to handle browser caching properly.  If you update the application, please clear your browser caches to avoid errors when using the web interface.

### Building from source

Built with Visual Studio 2022 (community edition).

Required dependencies:
  https://github.com/bp2008/broadlink-dotnet
  https://github.com/bp2008/BPUtil

Requires node.js.  After installing node.js, run `npm install` in the HotkeyAutomation project directory, or run the `Z_NpmInstall.bat` file.  Npm install must be run after updating the npm package list in package.json.
