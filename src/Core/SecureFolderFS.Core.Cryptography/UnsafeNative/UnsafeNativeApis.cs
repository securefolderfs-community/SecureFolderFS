using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SecureFolderFS.Core.Cryptography.UnsafeNative
{
    /// <summary>
    /// Provides access to native platform APIs for memory protection and other security operations.
    /// </summary>
    internal static class UnsafeNativeApis
    {
        #region Windows Memory Locking

        /// <summary>
        /// Locks the specified region of the process's virtual address space into physical memory,
        /// preventing the system from swapping the region to the paging file.
        /// </summary>
        /// <param name="lpAddress">A pointer to the base address of the region of pages to be locked.</param>
        /// <param name="dwSize">The size of the region to be locked, in bytes.</param>
        /// <returns>True if the function succeeds; otherwise, false.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [SupportedOSPlatform("windows")]
        public static extern bool VirtualLock(IntPtr lpAddress, nuint dwSize);

        /// <summary>
        /// Unlocks a specified range of pages in the virtual address space of a process,
        /// enabling the system to swap the pages out to the paging file if necessary.
        /// </summary>
        /// <param name="lpAddress">A pointer to the base address of the region of pages to be unlocked.</param>
        /// <param name="dwSize">The size of the region being unlocked, in bytes.</param>
        /// <returns>True if the function succeeds; otherwise, false.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [SupportedOSPlatform("windows")]
        public static extern bool VirtualUnlock(IntPtr lpAddress, nuint dwSize);

        #endregion

        #region Unix Memory Locking (Linux/macOS)

        /// <summary>
        /// Locks pages in the address range starting at addr and continuing for len bytes.
        /// All pages that contain a part of the specified address range are guaranteed to be
        /// resident in RAM when the call returns successfully.
        /// </summary>
        /// <param name="addr">The starting address of the memory region to lock.</param>
        /// <param name="len">The length of the memory region to lock.</param>
        /// <returns>0 on success; -1 on error.</returns>
        [DllImport("libc", SetLastError = true)]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        public static extern int mlock(IntPtr addr, nuint len);

        /// <summary>
        /// Unlocks pages in the address range starting at addr and continuing for len bytes.
        /// After this call, all pages that contain a part of the specified memory range can
        /// be moved to external swap space again by the kernel.
        /// </summary>
        /// <param name="addr">The starting address of the memory region to unlock.</param>
        /// <param name="len">The length of the memory region to unlock.</param>
        /// <returns>0 on success; -1 on error.</returns>
        [DllImport("libc", SetLastError = true)]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        public static extern int munlock(IntPtr addr, nuint len);

        #endregion
    }
}
