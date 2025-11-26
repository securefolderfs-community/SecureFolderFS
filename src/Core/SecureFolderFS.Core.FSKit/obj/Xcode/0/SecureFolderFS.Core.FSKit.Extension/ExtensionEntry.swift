//
//  ExtensionEntry.swift
//  SecureFolderFS.Core.FSKit
//
//  Created by d2dyno on 26/11/2025.
//

import Foundation
import FSKit
import ExtensionFoundation

// MARK: - .NET Library Imports

/// Function pointer types for .NET exports
typealias FSKitInitializeFunc = @convention(c) () -> Int32
typealias FSKitShutdownFunc = @convention(c) () -> Int32
typealias FSKitGetStatusFunc = @convention(c) () -> Int32

/// Loads the SecureFolderFS.Core.FSKit NativeAOT library
class DotNetLibraryLoader {
    private var libraryHandle: UnsafeMutableRawPointer?
    
    private var initializeFunc: FSKitInitializeFunc?
    private var shutdownFunc: FSKitShutdownFunc?
    private var getStatusFunc: FSKitGetStatusFunc?
    
    static let shared = DotNetLibraryLoader()
    
    private init() {}
    
    /// Loads the .NET library from the extension bundle
    func loadLibrary() -> Bool {
        guard let bundlePath = Bundle.main.bundlePath as NSString? else {
            NSLog("FSKit Extension: Failed to get bundle path")
            return false
        }
        
        // Try multiple possible locations for the dylib
        let possiblePaths = [
            bundlePath.appendingPathComponent("Contents/Resources/SecureFolderFS.Core.FSKit.dylib"),
            bundlePath.appendingPathComponent("SecureFolderFS.Core.FSKit.dylib"),
            bundlePath.appendingPathComponent("Contents/MacOS/SecureFolderFS.Core.FSKit.dylib")
        ]
        
        for libPath in possiblePaths {
            NSLog("FSKit Extension: Trying to load library from: \(libPath)")
            
            if let handle = dlopen(libPath, RTLD_NOW | RTLD_LOCAL) {
                self.libraryHandle = handle
                NSLog("FSKit Extension: Successfully loaded library from: \(libPath)")
                
                // Load function pointers
                if loadFunctionPointers() {
                    return true
                }
                
                dlclose(handle)
                self.libraryHandle = nil
            } else {
                if let error = dlerror() {
                    NSLog("FSKit Extension: Failed to load from \(libPath): \(String(cString: error))")
                }
            }
        }
        
        NSLog("FSKit Extension: Failed to load library from any location")
        return false
    }
    
    /// Loads function pointers from the loaded library
    private func loadFunctionPointers() -> Bool {
        guard let handle = libraryHandle else { return false }
        
        // Load FSKit_Initialize
        if let initPtr = dlsym(handle, "FSKit_Initialize") {
            self.initializeFunc = unsafeBitCast(initPtr, to: FSKitInitializeFunc.self)
        } else {
            NSLog("FSKit Extension: Failed to load FSKit_Initialize")
            return false
        }
        
        // Load FSKit_Shutdown
        if let shutdownPtr = dlsym(handle, "FSKit_Shutdown") {
            self.shutdownFunc = unsafeBitCast(shutdownPtr, to: FSKitShutdownFunc.self)
        } else {
            NSLog("FSKit Extension: Failed to load FSKit_Shutdown")
            return false
        }
        
        // Load FSKit_GetStatus
        if let statusPtr = dlsym(handle, "FSKit_GetStatus") {
            self.getStatusFunc = unsafeBitCast(statusPtr, to: FSKitGetStatusFunc.self)
        } else {
            NSLog("FSKit Extension: Failed to load FSKit_GetStatus")
            return false
        }
        
        NSLog("FSKit Extension: All function pointers loaded successfully")
        return true
    }
    
    /// Initializes the .NET library
    func initialize() -> Bool {
        guard let initFunc = initializeFunc else {
            NSLog("FSKit Extension: Initialize function not loaded")
            return false
        }
        
        let result = initFunc()
        if result == 0 {
            NSLog("FSKit Extension: .NET library initialized successfully")
            return true
        } else {
            NSLog("FSKit Extension: .NET library initialization failed with code: \(result)")
            return false
        }
    }
    
    /// Shuts down the .NET library
    func shutdown() {
        guard let shutdownFunc = shutdownFunc else { return }
        
        let result = shutdownFunc()
        if result == 0 {
            NSLog("FSKit Extension: .NET library shut down successfully")
        } else {
            NSLog("FSKit Extension: .NET library shutdown failed with code: \(result)")
        }
        
        if let handle = libraryHandle {
            dlclose(handle)
            libraryHandle = nil
        }
    }
    
    /// Gets the status of the .NET library
    func getStatus() -> Bool {
        guard let statusFunc = getStatusFunc else { return false }
        return statusFunc() == 1
    }
}

// MARK: - FSUnaryFileSystemExtension Protocol

@available(macOS 15.0, *)
@main
final class SecureFolderFSExtension: UnaryFileSystemExtension {
    
    var fileSystem : FSUnaryFileSystem & FSUnaryFileSystemOperations {
        NSLog("FSKit Extension: Creating file system for resource")
        return SecureFolderFileSystem()
    }
}

// MARK: - File System Implementation Proxy

@available(macOS 15.0, *)
class SecureFolderFileSystem: FSUnaryFileSystem, FSUnaryFileSystemOperations {
    
    override init() {
        super.init()
        NSLog("FSKit Extension: SecureFolderFileSystem initialized")
    }

    func probeResource(resource: FSResource, replyHandler reply: @escaping @Sendable (FSProbeResult?, (any Error)?) -> Void) {
        reply(
            FSProbeResult.usable(
                name: "Test1",
                containerID: FSContainerIdentifier(uuid: UUID(uuidString: "878CE919-1D7A-4D57-A7C2-C4B6E3EBC399")!)
            ), nil
        )
    }
    
    func loadResource(
        resource: FSResource,
        options: FSTaskOptions,
        replyHandler reply: @escaping @Sendable (FSVolume?, (any Error)?) -> Void) {
        NSLog("FSKit Extension: loadResource called")
        
        // Create a volume - FSKit will assign the mount point
        let volumeIdentifier = FSVolume.Identifier()
        let volumeName = FSFileName(string: "SecureFolderFS")
        let volume = FSVolume(volumeID: volumeIdentifier, volumeName: volumeName)
            
        // Load the .NET library on first use
        if !DotNetLibraryLoader.shared.getStatus() {
            NSLog("FSKit Extension: Loading .NET library...")
            
            guard DotNetLibraryLoader.shared.loadLibrary() else {
                NSLog("FSKit Extension: Failed to load .NET library")
                reply(nil, FSError(.moduleLoadFailed))
                return
            }
            
            guard DotNetLibraryLoader.shared.initialize() else {
                NSLog("FSKit Extension: Failed to initialize .NET library")
                reply(nil, FSError(.moduleLoadFailed))
                return
            }
        }
        
        // The actual file system operations are handled by the .NET library via IPC
        // This is just the native bridge
        reply(volume, nil)
        NSLog("FSKit Extension: Resource loaded successfully")
    }
    
    func unloadResource(resource: FSResource, options: FSTaskOptions, replyHandler reply: @escaping ((any Error)?) -> Void) {
        NSLog("FSKit Extension: unloadResource called")
        
        // Notify .NET library about unload via IPC if needed
        // The actual cleanup is handled by the .NET library
        reply(nil)
        DotNetLibraryLoader.shared.shutdown()
        
        NSLog("FSKit Extension: Resource unloaded successfully")
    }
}
