﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Storage.Blob;
using Microsoft.Azure.WebJobs.Host.Timers;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.WebJobs.Host.Loggers
{
    // Flush on a timer so that we get updated output.
    // Flush will come on a different thread, so we need to have thread-safe
    // access between the Reader (ToString)  and the Writers (which are happening as our
    // caller uses the textWriter that we return).
    internal sealed class UpdateOutputLogCommand : IRecurrentCommand, IDisposable, IFunctionOutput
    {
        // Contents for what's written. Owned by the timer thread.
        private readonly StringWriter _innerWriter;

        private readonly Func<string, CancellationToken, Task> _uploadCommand;

        // Thread-safe access to _innerWriter so that user threads can write to it. 
        private readonly TextWriter _synchronizedWriter;
        private object _writerSyncLock = new object();
        private bool _disposed;

        private UpdateOutputLogCommand(StringWriter innerWriter, Func<string, CancellationToken, Task> uploadCommand)
        {
            _innerWriter = innerWriter;
            _synchronizedWriter = TextWriter.Synchronized(_innerWriter);
            _uploadCommand = uploadCommand;
        }

        public IRecurrentCommand UpdateCommand
        {
            get
            {
                ThrowIfDisposed();
                return this;
            }
        }

        public TextWriter Output
        {
            get
            {
                ThrowIfDisposed();
                return _synchronizedWriter;
            }
        }

        public static Task<UpdateOutputLogCommand> CreateAsync(IStorageBlockBlob outputBlob, string existingContents,
            CancellationToken cancellationToken)
        {
            return CreateAsync(outputBlob, existingContents, (contents, innerToken) => UploadTextAsync(
                outputBlob, contents, innerToken), cancellationToken);
        }

        public static async Task<UpdateOutputLogCommand> CreateAsync(IStorageBlockBlob outputBlob,
            string existingContents, Func<string, CancellationToken, Task> uploadCommand,
            CancellationToken cancellationToken)
        {
            if (outputBlob == null)
            {
                throw new ArgumentNullException(nameof(outputBlob));
            }
            else if (uploadCommand == null)
            {
                throw new ArgumentNullException(nameof(uploadCommand));
            }

            StringWriter innerWriter = new StringWriter();

            if (existingContents != null)
            {
                // This can happen if the function was running previously and the 
                // node crashed. Save previous output, could be useful for diagnostics.
                innerWriter.WriteLine("Previous execution information:");
                innerWriter.WriteLine(existingContents);

                var lastTime = await GetBlobModifiedUtcTimeAsync(outputBlob, cancellationToken);
                if (lastTime.HasValue)
                {
                    var delta = DateTime.UtcNow - lastTime.Value;
                    innerWriter.WriteLine("... Last write at {0}, {1} ago", lastTime, delta);
                }

                innerWriter.WriteLine("========================");
            }

            return new UpdateOutputLogCommand(innerWriter, uploadCommand);
        }

        public async Task<bool> TryExecuteAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            // For synchronized text writer, the object is its own lock.
            string snapshot;

            lock (_writerSyncLock)
            {
                snapshot = _innerWriter.ToString();
            }

            await _uploadCommand.Invoke(snapshot, cancellationToken);
            return true;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _innerWriter.Dispose();
                _synchronizedWriter.Dispose();
                _disposed = true;
            }
        }

        public Task SaveAndCloseAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            string finalSnapshot;

            lock (_writerSyncLock)
            {
                _synchronizedWriter.Flush();
                finalSnapshot = _innerWriter.ToString();
                _synchronizedWriter.Close();
                _innerWriter.Close();
            }

            return _uploadCommand.Invoke(finalSnapshot, cancellationToken);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }

        private static Task UploadTextAsync(IStorageBlockBlob outputBlob, string contents,
            CancellationToken cancellationToken)
        {
            return outputBlob.UploadTextAsync(contents, cancellationToken: cancellationToken);
        }

        private static async Task<DateTime?> GetBlobModifiedUtcTimeAsync(IStorageBlob blob,
            CancellationToken cancellaitonToken)
        {
            if (!await blob.ExistsAsync(cancellaitonToken))
            {
                return null; // no blob, no time.
            }

            var props = blob.Properties;
            var time = props.LastModified;
            return time.HasValue ? (DateTime?)time.Value.UtcDateTime : null;
        }
    }
}
