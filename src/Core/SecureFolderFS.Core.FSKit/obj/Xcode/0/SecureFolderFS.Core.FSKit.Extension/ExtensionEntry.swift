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

        // Load volume operation functions
        if !VolumeOperationsBridge.shared.loadFunctions(from: handle) {
            NSLog("FSKit Extension: Failed to load volume operation functions")
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

// MARK: - C# Volume Bridge

/// Function pointer types for volume operations (updated for pointer parameters)
typealias FSVolumeCreateFunc = @convention(c) (UnsafePointer<UInt8>, Int32) -> UnsafeMutableRawPointer?
typealias FSVolumeDestroyFunc = @convention(c) (UnsafeMutableRawPointer) -> Void
typealias FSVolumeActivateFunc = @convention(c) (UnsafeMutableRawPointer) -> Int32
typealias FSVolumeDeactivateFunc = @convention(c) (UnsafeMutableRawPointer) -> Int32
typealias FSVolumeUnmountFunc = @convention(c) (UnsafeMutableRawPointer) -> Int32
typealias FSVolumeLookupFunc = @convention(c) (UnsafeMutableRawPointer, UnsafePointer<UInt8>, Int32, Int64, UnsafeMutablePointer<Int64>) -> Int32
typealias FSVolumeGetAttributesFunc = @convention(c) (UnsafeMutableRawPointer, Int64, UnsafeMutableRawPointer?) -> Int32
typealias FSVolumeReadFunc = @convention(c) (UnsafeMutableRawPointer, Int64, Int64, UnsafeMutableRawPointer, Int32, UnsafeMutablePointer<Int32>) -> Int32
typealias FSVolumeWriteFunc = @convention(c) (UnsafeMutableRawPointer, Int64, Int64, UnsafeRawPointer, Int32, UnsafeMutablePointer<Int32>) -> Int32
typealias FSVolumeCreateItemFunc = @convention(c) (UnsafeMutableRawPointer, UnsafePointer<UInt8>, Int32, Int32, Int64, UnsafeMutablePointer<Int64>) -> Int32
typealias FSVolumeRemoveItemFunc = @convention(c) (UnsafeMutableRawPointer, Int64, UnsafePointer<UInt8>, Int32, Int64) -> Int32
typealias FSVolumeEnumerateFunc = @convention(c) (UnsafeMutableRawPointer, Int64, Int64, UnsafeMutableRawPointer?) -> Int32

/// Manages access to C# volume functions
class VolumeOperationsBridge {
    private var createFunc: FSVolumeCreateFunc?
    private var destroyFunc: FSVolumeDestroyFunc?
    private var activateFunc: FSVolumeActivateFunc?
    private var deactivateFunc: FSVolumeDeactivateFunc?
    private var unmountFunc: FSVolumeUnmountFunc?
    private var lookupFunc: FSVolumeLookupFunc?
    private var getAttributesFunc: FSVolumeGetAttributesFunc?
    private var readFunc: FSVolumeReadFunc?
    private var writeFunc: FSVolumeWriteFunc?
    private var createItemFunc: FSVolumeCreateItemFunc?
    private var removeItemFunc: FSVolumeRemoveItemFunc?
    private var enumerateFunc: FSVolumeEnumerateFunc?

    static let shared = VolumeOperationsBridge()

    private init() {}

    func loadFunctions(from handle: UnsafeMutableRawPointer) -> Bool {
        // Load all function pointers
        if let ptr = dlsym(handle, "FSVolume_Create") {
            createFunc = unsafeBitCast(ptr, to: FSVolumeCreateFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_Destroy") {
            destroyFunc = unsafeBitCast(ptr, to: FSVolumeDestroyFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_Activate") {
            activateFunc = unsafeBitCast(ptr, to: FSVolumeActivateFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_Deactivate") {
            deactivateFunc = unsafeBitCast(ptr, to: FSVolumeDeactivateFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_Unmount") {
            unmountFunc = unsafeBitCast(ptr, to: FSVolumeUnmountFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_LookupItem") {
            lookupFunc = unsafeBitCast(ptr, to: FSVolumeLookupFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_GetAttributes") {
            getAttributesFunc = unsafeBitCast(ptr, to: FSVolumeGetAttributesFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_Read") {
            readFunc = unsafeBitCast(ptr, to: FSVolumeReadFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_Write") {
            writeFunc = unsafeBitCast(ptr, to: FSVolumeWriteFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_CreateItem") {
            createItemFunc = unsafeBitCast(ptr, to: FSVolumeCreateItemFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_RemoveItem") {
            removeItemFunc = unsafeBitCast(ptr, to: FSVolumeRemoveItemFunc.self)
        } else { return false }

        if let ptr = dlsym(handle, "FSVolume_EnumerateDirectory") {
            enumerateFunc = unsafeBitCast(ptr, to: FSVolumeEnumerateFunc.self)
        } else { return false }

        NSLog("FSKit Extension: All volume operation functions loaded")
        return true
    }

    // Wrapper methods
    func createVolume(name: String) -> UnsafeMutableRawPointer? {
        guard let createFunc = createFunc else { return nil }
        let nameData = name.data(using: .utf8)!
        return nameData.withUnsafeBytes { bytes in
            createFunc(bytes.baseAddress!.assumingMemoryBound(to: UInt8.self), Int32(nameData.count))
        }
    }

    func destroyVolume(_ handle: UnsafeMutableRawPointer) {
        destroyFunc?(handle)
    }

    func activate(_ handle: UnsafeMutableRawPointer) -> Int32 {
        return activateFunc?(handle) ?? -1
    }

    func deactivate(_ handle: UnsafeMutableRawPointer) -> Int32 {
        return deactivateFunc?(handle) ?? -1
    }

    func unmount(_ handle: UnsafeMutableRawPointer) -> Int32 {
        return unmountFunc?(handle) ?? -1
    }
    
    func lookupItem(handle: UnsafeMutableRawPointer, name: String, directoryId: Int64) -> (Int32, Int64) {
        guard let lookupFunc = lookupFunc else { return (-1, 0) }
        
        let nameData = name.data(using: .utf8)!
        var itemId: Int64 = 0
        
        let result = nameData.withUnsafeBytes { bytes in
            lookupFunc(handle, bytes.baseAddress!.assumingMemoryBound(to: UInt8.self), Int32(nameData.count), directoryId, &itemId)
        }
        
        return (result, itemId)
    }
    
    func getAttributes(handle: UnsafeMutableRawPointer, itemId: Int64) -> Int32 {
        guard let getAttributesFunc = getAttributesFunc else { return -1 }
        return getAttributesFunc(handle, itemId, nil)
    }
    
    func read(handle: UnsafeMutableRawPointer, itemId: Int64, offset: Int64, buffer: UnsafeMutableRawPointer, length: Int32) -> (Int32, Int32) {
        guard let readFunc = readFunc else { return (-1, 0) }
        
        var bytesRead: Int32 = 0
        let result = readFunc(handle, itemId, offset, buffer, length, &bytesRead)
        
        return (result, bytesRead)
    }
    
    func write(handle: UnsafeMutableRawPointer, itemId: Int64, offset: Int64, buffer: UnsafeRawPointer, length: Int32) -> (Int32, Int32) {
        guard let writeFunc = writeFunc else { return (-1, 0) }
        
        var bytesWritten: Int32 = 0
        let result = writeFunc(handle, itemId, offset, buffer, length, &bytesWritten)
        
        return (result, bytesWritten)
    }
    
    func createItem(handle: UnsafeMutableRawPointer, name: String, itemType: Int32, directoryId: Int64) -> (Int32, Int64) {
        guard let createItemFunc = createItemFunc else { return (-1, 0) }
        
        let nameData = name.data(using: .utf8)!
        var newItemId: Int64 = 0
        
        let result = nameData.withUnsafeBytes { bytes in
            createItemFunc(handle, bytes.baseAddress!.assumingMemoryBound(to: UInt8.self), Int32(nameData.count), itemType, directoryId, &newItemId)
        }
        
        return (result, newItemId)
    }
    
    func removeItem(handle: UnsafeMutableRawPointer, itemId: Int64, name: String, directoryId: Int64) -> Int32 {
        guard let removeItemFunc = removeItemFunc else { return -1 }
        
        let nameData = name.data(using: .utf8)!
        
        return nameData.withUnsafeBytes { bytes in
            removeItemFunc(handle, itemId, bytes.baseAddress!.assumingMemoryBound(to: UInt8.self), Int32(nameData.count), directoryId)
        }
    }
    
    func enumerateDirectory(handle: UnsafeMutableRawPointer, directoryId: Int64, startingCookie: Int64) -> Int32 {
        guard let enumerateFunc = enumerateFunc else { return -1 }
        return enumerateFunc(handle, directoryId, startingCookie, nil)
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
                name: "SecureFolderFS",
                containerID: FSContainerIdentifier(uuid: UUID(uuidString: "878CE919-1D7A-4D57-A7C2-C4B6E3EBC399")!)
            ), nil
        )
    }

    func loadResource(
        resource: FSResource,
        options: FSTaskOptions,
        replyHandler reply: @escaping @Sendable (FSVolume?, (any Error)?) -> Void) {
        NSLog("FSKit Extension: loadResource called")

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

        // Create our custom volume that bridges to C#
        let volumeIdentifier = FSVolume.Identifier()
        let volumeName = FSFileName(string: "SecureFolderFS")
        let volume = SecureFolderVolume(volumeID: volumeIdentifier, volumeName: volumeName)

        // The actual file system operations are handled by the C# library
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

// MARK: - C# Volume Bridge Implementation

/// FSVolume subclass that bridges to C# implementation
@available(macOS 15.0, *)
class SecureFolderVolume: FSVolume {
    private var volumeHandle: UnsafeMutableRawPointer?
    private let root: FSItem = {
        let item = FSItem()
        return item
    }()

    override init(volumeID: FSVolume.Identifier, volumeName: FSFileName) {
        super.init(volumeID: volumeID, volumeName: volumeName)

        // Create C# volume instance
        if let handle = VolumeOperationsBridge.shared.createVolume(name: volumeName.string!) {
            self.volumeHandle = handle
            NSLog("FSKit Extension: Created C# volume handle: \(handle)")
        } else {
            NSLog("FSKit Extension: Failed to create C# volume handle")
        }
    }

    deinit {
        if let handle = volumeHandle {
            VolumeOperationsBridge.shared.destroyVolume(handle)
            NSLog("FSKit Extension: Destroyed C# volume handle")
        }
    }
}

extension SecureFolderVolume: FSVolume.Operations {
    
    var supportedVolumeCapabilities: FSVolume.SupportedCapabilities {
        let capabilities = FSVolume.SupportedCapabilities()
        capabilities.supportsHardLinks = true
        capabilities.supportsSymbolicLinks = true
        capabilities.supportsPersistentObjectIDs = true
        capabilities.doesNotSupportVolumeSizes = true
        capabilities.supportsHiddenFiles = true
        capabilities.supports64BitObjectIDs = true
        capabilities.caseFormat = .insensitiveCasePreserving
        return capabilities
    }
    
    var volumeStatistics: FSStatFSResult {
        let result = FSStatFSResult(fileSystemTypeName: "SecureFolderVolume")
        
        result.blockSize = 4096
        result.ioSize = 4096
        result.totalBlocks = 1000000
        result.availableBlocks = 500000
        result.freeBlocks = 500000
        result.totalFiles = 1000
        result.freeFiles = 1000
        
        return result
    }
    
    func mount(options: FSTaskOptions, replyHandler reply: @escaping @Sendable ((any Error)?) -> Void) {
        NSLog("FSKit Extension: mount called")
        reply(nil)
    }
    
    func unmount(replyHandler reply: @escaping @Sendable () -> Void) {
        NSLog("FSKit Extension: unmount called")
        if let handle = volumeHandle {
            _ = VolumeOperationsBridge.shared.unmount(handle)
        }
        reply()
    }
    
    func synchronize(flags: FSSyncFlags, replyHandler reply: @escaping @Sendable ((any Error)?) -> Void) {
        NSLog("FSKit Extension: synchronize called")
        reply(nil)
    }
    
    func getAttributes(_ desiredAttributes: FSItem.GetAttributesRequest, of item: FSItem, replyHandler reply: @escaping @Sendable (FSItem.Attributes?, (any Error)?) -> Void) {
        guard let handle = volumeHandle else {
            reply(nil, NSError(domain: NSPOSIXErrorDomain, code: Int(EINVAL), userInfo: nil))
            return
        }
        
        let result = VolumeOperationsBridge.shared.getAttributes(handle: handle, itemId: item.attributes.fileID)
        
        if result == 0 {
            // Create default attributes - extend with actual C# data later
            let attrs = FSItem.Attributes()
            reply(attrs, nil)
        } else {
            reply(nil, NSError(domain: NSPOSIXErrorDomain, code: Int(-result), userInfo: nil))
        }
    }
    
    func setAttributes(_ newAttributes: FSItem.SetAttributesRequest, on item: FSItem, replyHandler reply: @escaping @Sendable (FSItem.Attributes?, (any Error)?) -> Void) {
        NSLog("FSKit Extension: setAttributes called")
        // Not implemented yet
        reply(nil, NSError(domain: NSPOSIXErrorDomain, code: Int(ENOSYS), userInfo: nil))
    }
    
    func lookupItem(named name: FSFileName, inDirectory directory: FSItem, replyHandler reply: @escaping @Sendable (FSItem?, FSFileName?, (any Error)?) -> Void) {
        guard let handle = volumeHandle, let nameStr = name.string else {
            reply(nil, nil, NSError(domain: NSPOSIXErrorDomain, code: Int(EINVAL), userInfo: nil))
            return
        }
        
        let (result, itemId) = VolumeOperationsBridge.shared.lookupItem(
            handle: handle,
            name: nameStr,
            directoryId: directory.attributes.fileID
        )
        
        if result == 0 {
            let item = FSItem()
            item.attributes.fileID = itemId
            reply(item, name, nil)
        } else {
            reply(nil, nil, NSError(domain: NSPOSIXErrorDomain, code: Int(-result), userInfo: nil))
        }
    }
    
    func reclaimItem(_ item: FSItem, replyHandler reply: @escaping @Sendable ((any Error)?) -> Void) {
        NSLog("FSKit Extension: reclaimItem called")
        reply(nil)
    }
    
    func readSymbolicLink(_ item: FSItem, replyHandler reply: @escaping @Sendable (FSFileName?, (any Error)?) -> Void) {
        NSLog("FSKit Extension: readSymbolicLink called")
        reply(nil, NSError(domain: NSPOSIXErrorDomain, code: Int(EINVAL), userInfo: nil))
    }
    
    func createItem(named name: FSFileName, type: FSItem.ItemType, inDirectory directory: FSItem, attributes newAttributes: FSItem.SetAttributesRequest, replyHandler reply: @escaping @Sendable (FSItem?, FSFileName?, (any Error)?) -> Void) {
        guard let handle = volumeHandle, let nameStr = name.string else {
            reply(nil, nil, NSError(domain: NSPOSIXErrorDomain, code: Int(EINVAL), userInfo: nil))
            return
        }
        
        let itemType: Int32 = type == .directory ? 1 : 0
        let (result, newItemId) = VolumeOperationsBridge.shared.createItem(
            handle: handle,
            name: nameStr,
            itemType: itemType,
            directoryId: directory.attributes.fileID
        )
        
        if result == 0 {
            let item = FSItem()
            item.attributes.fileID = newItemId
            reply(item, name, nil)
        } else {
            reply(nil, nil, NSError(domain: NSPOSIXErrorDomain, code: Int(-result), userInfo: nil))
        }
    }
    
    func createSymbolicLink(named name: FSFileName, inDirectory directory: FSItem, attributes newAttributes: FSItem.SetAttributesRequest, linkContents contents: FSFileName, replyHandler reply: @escaping @Sendable (FSItem?, FSFileName?, (any Error)?) -> Void) {
        NSLog("FSKit Extension: createSymbolicLink called")
        reply(nil, nil, NSError(domain: NSPOSIXErrorDomain, code: Int(ENOSYS), userInfo: nil))
    }
    
    func createLink(to item: FSItem, named name: FSFileName, inDirectory directory: FSItem, replyHandler reply: @escaping @Sendable (FSFileName?, (any Error)?) -> Void) {
        NSLog("FSKit Extension: createLink called")
        reply(nil, NSError(domain: NSPOSIXErrorDomain, code: Int(ENOSYS), userInfo: nil))
    }
    
    func removeItem(_ item: FSItem, named name: FSFileName, fromDirectory directory: FSItem, replyHandler reply: @escaping @Sendable ((any Error)?) -> Void) {
        guard let handle = volumeHandle, let nameStr = name.string else {
            reply(NSError(domain: NSPOSIXErrorDomain, code: Int(EINVAL), userInfo: nil))
            return
        }
        
        let result = VolumeOperationsBridge.shared.removeItem(
            handle: handle,
            itemId: item.attributes.fileID,
            name: nameStr,
            directoryId: directory.attributes.fileID
        )
        
        if result == 0 {
            reply(nil)
        } else {
            reply(NSError(domain: NSPOSIXErrorDomain, code: Int(-result), userInfo: nil))
        }
    }
    
    func renameItem(_ item: FSItem, inDirectory sourceDirectory: FSItem, named sourceName: FSFileName, to destinationName: FSFileName, inDirectory destinationDirectory: FSItem, overItem: FSItem?, replyHandler reply: @escaping @Sendable (FSFileName?, (any Error)?) -> Void) {
        NSLog("FSKit Extension: renameItem called")
        reply(nil, NSError(domain: NSPOSIXErrorDomain, code: Int(ENOSYS), userInfo: nil))
    }
    
    func enumerateDirectory(_ directory: FSItem,
                            startingAt cookie: FSDirectoryCookie,
                            verifier: FSDirectoryVerifier,
                            attributes: FSItem.GetAttributesRequest?,
                            packer: FSDirectoryEntryPacker,
                            replyHandler reply: @escaping @Sendable (FSDirectoryVerifier, (any Error)?) -> Void) {
        guard let handle = volumeHandle else {
            reply(verifier, NSError(domain: NSPOSIXErrorDomain, code: Int(EINVAL), userInfo: nil))
            return
        }
        
        let result = VolumeOperationsBridge.shared.enumerateDirectory(
            handle: handle,
            directoryId: directory.attributes.fileID,
            startingCookie: cookie.numericValue
        )
        
        if result == 0 {
            reply(verifier, nil)
        } else {
            reply(verifier, NSError(domain: NSPOSIXErrorDomain, code: Int(-result), userInfo: nil))
        }
    }
    
    func activate(options: FSTaskOptions, replyHandler reply: @escaping @Sendable (FSItem?, (any Error)?) -> Void) {
        guard let handle = volumeHandle else {
            reply(nil, NSError(domain: NSPOSIXErrorDomain, code: Int(EINVAL), userInfo: nil))
            return
        }
        
        let result = VolumeOperationsBridge.shared.activate(handle)
        if result == 0 {
            reply(root, nil)
        } else {
            reply(nil, NSError(domain: NSPOSIXErrorDomain, code: Int(-result), userInfo: nil))
        }
    }
    
    func deactivate(options: FSDeactivateOptions = [], replyHandler reply: @escaping @Sendable ((any Error)?) -> Void) {
        guard let handle = volumeHandle else {
            reply(NSError(domain: NSPOSIXErrorDomain, code: Int(EINVAL), userInfo: nil))
            return
        }
        
        let result = VolumeOperationsBridge.shared.deactivate(handle)
        if result == 0 {
            reply(nil)
        } else {
            reply(NSError(domain: NSPOSIXErrorDomain, code: Int(-result), userInfo: nil))
        }
    }
    
    var maximumLinkCount: Int {
        return 1024
    }
    
    var maximumNameLength: Int {
        return 255
    }
    
    var restrictsOwnershipChanges: Bool {
        return true
    }
    
    var truncatesLongNames: Bool {
        return false
    }
}
