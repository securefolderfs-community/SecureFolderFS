using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Helpers.Health
{
    public static partial class HealthHelpers
    {
        /// <summary>
        /// Represents the result of a file content validation.
        /// </summary>
        public sealed class FileContentValidationResult
        {
            /// <summary>
            /// Gets whether the file header is valid and readable.
            /// </summary>
            public bool IsHeaderValid { get; init; }

            /// <summary>
            /// Gets the list of chunk numbers that failed validation.
            /// </summary>
            public IReadOnlyList<long> CorruptedChunks { get; init; } = Array.Empty<long>();

            /// <summary>
            /// Gets whether the file is recoverable (header is valid but some chunks are corrupted).
            /// </summary>
            public bool IsRecoverable => IsHeaderValid && CorruptedChunks.Count > 0;

            /// <summary>
            /// Gets whether the file is irrecoverable (header is invalid).
            /// </summary>
            public bool IsIrrecoverable => !IsHeaderValid;

            /// <summary>
            /// Gets whether the file is completely valid.
            /// </summary>
            public bool IsValid => IsHeaderValid && CorruptedChunks.Count == 0;
        }

        /// <summary>
        /// Validates the contents of an encrypted file, checking both header and chunk integrity.
        /// </summary>
        /// <param name="file">The encrypted file to validate.</param>
        /// <param name="security">The security object for decryption.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="FileContentValidationResult"/> containing the validation results.</returns>
        public static async Task<FileContentValidationResult> ValidateFileContentsAsync(IFile file, Security security, CancellationToken cancellationToken = default)
        {
            await using var stream = await file.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);

            // Check if file is empty or too small to have a header
            if (stream.Length < security.HeaderCrypt.HeaderCiphertextSize)
            {
                return new FileContentValidationResult
                {
                    IsHeaderValid = stream.Length == 0, // Empty files are considered valid
                    CorruptedChunks = Array.Empty<long>()
                };
            }

            // Try to read and decrypt the header
            var headerBuffer = new HeaderBuffer(security.HeaderCrypt.HeaderPlaintextSize);
            try
            {
                var ciphertextHeader = new byte[security.HeaderCrypt.HeaderCiphertextSize];
                var read = await stream.ReadAsync(ciphertextHeader, cancellationToken);

                if (read < ciphertextHeader.Length)
                {
                    return new FileContentValidationResult
                    {
                        IsHeaderValid = false,
                        CorruptedChunks = Array.Empty<long>()
                    };
                }

                var headerDecrypted = security.HeaderCrypt.DecryptHeader(ciphertextHeader, headerBuffer);
                if (!headerDecrypted)
                {
                    return new FileContentValidationResult
                    {
                        IsHeaderValid = false,
                        CorruptedChunks = Array.Empty<long>()
                    };
                }
            }
            catch (ArgumentException)
            {
                return new FileContentValidationResult
                {
                    IsHeaderValid = false,
                    CorruptedChunks = Array.Empty<long>()
                };
            }
            catch (CryptographicException)
            {
                return new FileContentValidationResult
                {
                    IsHeaderValid = false,
                    CorruptedChunks = Array.Empty<long>()
                };
            }

            // Header is valid, now check chunks
            var corruptedChunks = new List<long>();
            var ciphertextChunkSize = security.ContentCrypt.ChunkCiphertextSize;
            var plaintextChunkSize = security.ContentCrypt.ChunkPlaintextSize;
            var ciphertextChunk = ArrayPool<byte>.Shared.Rent(ciphertextChunkSize);
            var plaintextChunk = ArrayPool<byte>.Shared.Rent(plaintextChunkSize);

            try
            {
                long chunkNumber = 0;

                while (stream.Position < stream.Length)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var read = await stream.ReadAsync(ciphertextChunk.AsMemory(0, ciphertextChunkSize), cancellationToken);
                    if (read == 0)
                        break;

                    // Check if chunk first bytes are all zeros (extended chunk, skip validation)
                    var chunkReservedSize = Math.Min(read, security.ContentCrypt.ChunkFirstReservedSize);
                    var isAllZeros = true;
                    for (var i = 0; i < chunkReservedSize; i++)
                    {
                        if (ciphertextChunk[i] != 0)
                        {
                            isAllZeros = false;
                            break;
                        }
                    }

                    if (!isAllZeros)
                    {
                        // Try to decrypt the chunk
                        var decryptResult = security.ContentCrypt.DecryptChunk(
                            ciphertextChunk.AsSpan(0, read),
                            chunkNumber,
                            headerBuffer,
                            plaintextChunk);

                        if (!decryptResult)
                        {
                            corruptedChunks.Add(chunkNumber);
                        }
                    }

                    chunkNumber++;
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(ciphertextChunk.AsSpan(0, ciphertextChunkSize));
                CryptographicOperations.ZeroMemory(plaintextChunk.AsSpan(0, plaintextChunkSize));
                ArrayPool<byte>.Shared.Return(ciphertextChunk);
                ArrayPool<byte>.Shared.Return(plaintextChunk);
            }

            return new FileContentValidationResult
            {
                IsHeaderValid = true,
                CorruptedChunks = corruptedChunks
            };
        }

        /// <summary>
        /// Repairs corrupted chunks in a file by zeroing them out.
        /// </summary>
        /// <param name="file">The file to repair.</param>
        /// <param name="security">The security object for encryption.</param>
        /// <param name="corruptedChunks">The list of chunk numbers that are corrupted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task<IResult> RepairFileChunksAsync(IFile file, Security security, IReadOnlyList<long> corruptedChunks, CancellationToken cancellationToken = default)
        {
            if (corruptedChunks.Count == 0)
                return Result.Success;

            try
            {
                await using var stream = await file.OpenStreamAsync(FileAccess.ReadWrite, FileShare.None, cancellationToken);

                var headerSize = security.HeaderCrypt.HeaderCiphertextSize;
                var ciphertextChunkSize = security.ContentCrypt.ChunkCiphertextSize;

                // Zero buffer for writing
                var zeroBuffer = new byte[ciphertextChunkSize];

                foreach (var chunkNumber in corruptedChunks)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var chunkPosition = headerSize + (chunkNumber * ciphertextChunkSize);

                    // Check if position is within file bounds
                    if (chunkPosition >= stream.Length)
                        continue;

                    // Calculate how much to write (might be less for the last chunk)
                    var remainingBytes = stream.Length - chunkPosition;
                    var bytesToWrite = (int)Math.Min(ciphertextChunkSize, remainingBytes);

                    // Seek to chunk position and zero it out
                    stream.Position = chunkPosition;
                    await stream.WriteAsync(zeroBuffer.AsMemory(0, bytesToWrite), cancellationToken);
                }

                await stream.FlushAsync(cancellationToken);
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }

        /// <summary>
        /// Deletes a file that has an irrecoverable header.
        /// </summary>
        /// <param name="file">The file to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task<IResult> DeleteIrrecoverableFileAsync(IFile file, CancellationToken cancellationToken = default)
        {
            try
            {
                if (file is not IChildFile childFile)
                    return Result.Failure(new InvalidOperationException("File is not a child file."));

                var parent = await childFile.GetParentAsync(cancellationToken);
                if (parent is not IModifiableFolder modifiableFolder)
                    return Result.Failure(new InvalidOperationException("Parent folder does not support deletion."));

                await modifiableFolder.DeleteAsync(childFile, cancellationToken);
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }
    }
}

