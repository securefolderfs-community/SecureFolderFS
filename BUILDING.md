### Cloning

Open a terminal window and paste the following command:
```ps
git clone --recursive https://github.com/securefolderfs-community/SecureFolderFS.git
```
Running this command will clone the repository with all submodules into the current directory.

## Building WinUI
### 1. Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the following components installed:
  - .NET 7 SDK
  - Windows 11 SDK (10.0.22000.0)
  - MSVC v143 - VS 2022 C++ x64/x86 or ARM64 build tools (latest)
- [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads#current-releases)

### 2. Preparing workload
- Open SecureFolderFS.sln file which will launch Visual Studio.
- Ensure that the following build settings match your configuration (toolbar):
  - x64, x86, arm64 depending on your processor's architecture
  - Startup project set to SecureFolderFS.WinUI (You can change the startup project by opening Solution Explorer > Right click SecureFolderFS.WinUI > "Set as startup project"
  ![image](https://user-images.githubusercontent.com/53011783/216186419-aed03f32-565a-469d-9815-b7ea9206bf57.png)
  
## Building AvaloniaUI
### 1. Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or other editor of choice.
  - .NET 7 SDK

### 2. Preparing workload
> **Note**
> The following instructions apply to Visual Studio 2022

- Open SecureFolderFS.sln file which will launch Visual Studio.
- Ensure that the following build settings match your configuration (toolbar):
  - Selected "Any CPU"
  - Startup project set to SecureFolderFS.AvaloniaUI (You can change the startup project by opening Solution Explorer > Right click SecureFolderFS.AvaloniaUI > "Set as startup project"
  ![image](https://user-images.githubusercontent.com/53011783/216189292-474db056-0e3b-419d-baae-e86e27b7a7e7.png)
