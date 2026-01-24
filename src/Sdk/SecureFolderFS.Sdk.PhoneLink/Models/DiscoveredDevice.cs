using System;

namespace SecureFolderFS.Sdk.PhoneLink.Models;

/// <summary>
/// Represents a discovered mobile device.
/// </summary>
public class DiscoveredDevice
{
    /// <summary>
    /// Unique identifier for this device.
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// User-friendly name of the device.
    /// </summary>
    public required string DeviceName { get; init; }

    /// <summary>
    /// IP address of the device.
    /// </summary>
    public required string IpAddress { get; init; }

    /// <summary>
    /// Port for TCP communication.
    /// </summary>
    public required int Port { get; init; }

    /// <summary>
    /// Public key of the device (for verification).
    /// </summary>
    public byte[]? PublicKey { get; init; }

    /// <summary>
    /// Time when the device was discovered.
    /// </summary>
    public DateTime DiscoveredAt { get; init; } = DateTime.UtcNow;

    public override string ToString() => $"{DeviceName} ({IpAddress}:{Port})";
}