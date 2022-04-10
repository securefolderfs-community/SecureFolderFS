﻿using SecureFolderFS.Sdk.Streams;
using System;

namespace SecureFolderFS.Core.Streams.Management
{
    internal sealed class CiphertextStreamsManager : IDisposable
    {
        private readonly StreamsManager<ICiphertextFileStream> _readOnlyStreams;

        private readonly StreamsManager<ICiphertextFileStream> _readWriteStreams;

        private bool _disposed;

        public CiphertextStreamsManager()
        {
            this._readOnlyStreams = new StreamsManager<ICiphertextFileStream>();
            this._readWriteStreams = new StreamsManager<ICiphertextFileStream>();
        }

        public void AddStream(ICiphertextFileStream ciphertextFileStream)
        {
            AssertNotDisposed();

            _readOnlyStreams.AddStream(ciphertextFileStream);
            if (ciphertextFileStream.CanWrite)
            {
                _readWriteStreams.AddStream(ciphertextFileStream);
            }
        }

        public void RemoveStream(ICiphertextFileStream ciphertextFileStream)
        {
            AssertNotDisposed();

            _readOnlyStreams.RemoveStream(ciphertextFileStream);
            _readWriteStreams.RemoveStream(ciphertextFileStream);
        }

        public ICiphertextFileStream EnsureReadOnlyStreamInstance(ICiphertextFileStream ciphertextFileStream = null)
        {
            AssertNotDisposed();

            if (ciphertextFileStream == null || ciphertextFileStream.IsDisposed)
            {
                return _readOnlyStreams.GetAvailableStream();
            }

            return ciphertextFileStream;
        }

        public ICiphertextFileStream EnsureReadWriteStreamInstance(ICiphertextFileStream ciphertextFileStream = null)
        {
            AssertNotDisposed();

            if (ciphertextFileStream == null || ciphertextFileStream.IsDisposed)
            {
                return _readWriteStreams.GetAvailableStream();
            }

            return ciphertextFileStream;
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _readOnlyStreams.Dispose();
            _readWriteStreams.Dispose();
        }
    }
}
