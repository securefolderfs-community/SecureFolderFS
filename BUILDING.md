### Cloning

Open a new terminal window and paste the following command:
```ps
git clone --recursive https://github.com/securefolderfs-community/SecureFolderFS.git
```
Running this command will clone the repository with all submodules into the current directory.

## Building WinUI
### 1. Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the following components installed:
  - .NET 7 SDK
  - Windows 11 SDK (10.0.22621.0)
  - MSVC v143 - VS 2022 C++ x64/x86 or ARM64 build tools (latest)
- [Single-project MSIX Packaging Tools for VS 2022](https://marketplace.visualstudio.com/items?itemName=ProjectReunion.MicrosoftSingleProjectMSIXPackagingToolsDev17) extension
- [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads#current-releases)

### 2. Preparing workload
- Open the SecureFolderFS.sln file which will launch Visual Studio.
- Ensure that the following build settings match your configuration (toolbar):
  - x64, x86, arm64 depending on your processor's architecture
  - Startup project set to SecureFolderFS.WinUI (You can change the startup project by opening Solution Explorer > Right-click SecureFolderFS.WinUI > "Set as startup project"
  ![image](https://user-images.githubusercontent.com/53011783/216186419-aed03f32-565a-469d-9815-b7ea9206bf57.png)
  
## Building AvaloniaUI

### 1. Prerequisites
- .NET 7 SDK
- Optionally, [Visual Studio 2022](https://visualstudio.microsoft.com/vs/), [Rider](https://www.jetbrains.com/rider), or other editor of choice

### 2. Preparing workload
#### Building with Visual Studio 2022
  - Open the SecureFolderFS.sln file which will launch Visual Studio
  - Ensure that the following build settings match your configuration (toolbar):
    - Selected "Any CPU"
    - Startup project set to SecureFolderFS.AvaloniaUI (You can change the startup project by opening Solution Explorer > Right-click SecureFolderFS.AvaloniaUI > "Set as startup project"<br/>
    ![image](https://user-images.githubusercontent.com/53011783/216189292-474db056-0e3b-419d-baae-e86e27b7a7e7.png)
  
#### Building with Rider
  - Launch Rider
  - In the welcome screen, click "Open" and select SecureFolderFS.sln
  - Ensure that the following build settings match your configuration (toolbar):
    - Selected "Any CPU"<br/>
    ![image](https://github.com/securefolderfs-community/SecureFolderFS/assets/79316397/52f671bc-5c37-4d82-b1fa-f4fb53ada8ef)

    - Startup project set to SecureFolderFS.AvaloniaUI<br/>
    ![image](https://github.com/securefolderfs-community/SecureFolderFS/assets/79316397/3a26ba05-db0a-4d1d-ae08-0d5f16072092)
#### Building with Terminal

Open a terminal window and paste the following command:
```ps
dotnet build SecureFolderFS.AvaloniaUI
```

To build the Release version of the app, modify the command as follows:
```ps
dotnet build SecureFolderFS.AvaloniaUI -c Release
```
