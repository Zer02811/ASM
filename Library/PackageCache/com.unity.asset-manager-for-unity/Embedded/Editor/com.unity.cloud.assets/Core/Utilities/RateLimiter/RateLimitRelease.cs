// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// From https://github.com/dotnet/runtime/ at commit 06062e79faab44195f56cd7e4079b22d2380aedd
#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Abstraction for leases returned by <see cref="RateLimiter"/> implementations.
    /// A lease represents the success or failure to acquire a resource and contains potential metadata that is relevant to the acquisition operation.
    /// </summary>
    internal abstract class RateLimitLease : IDisposable
    {
        /// <summary>
        /// Represents whether lease acquisition was successful.
        /// </summary>
        public abstract bool IsAcquired { get; }

        /// <summary>
        /// Attempt to extract metadata for the lease.
        /// </summary>
        /// <param name="metadataName">The name of the metadata. Some common ones can be found in <see cref="MetadataName"/>.</param>
        /// <param name="metadata">The metadata object if it exists.</param>
        /// <returns>True if the metadata exists, otherwise false.</returns>
        public abstract bool TryGetMetadata(string metadataName, out object? metadata);

        /// <summary>
        /// Attempt to extract a strongly-typed metadata for the lease.
        /// </summary>
        /// <typeparam name="T">Type of the expected metadata.</typeparam>
        /// <param name="metadataName">The name of the strongly-typed metadata. Some common ones can be found in <see cref="MetadataName"/>.</param>
        /// <param name="metadata">The strongly-typed metadata object if it exists.</param>
        /// <returns>True if the metadata exists, otherwise false.</returns>
        public bool TryGetMetadata<T>(MetadataName<T> metadataName, [MaybeNull] out T metadata)
        {
            if (metadataName.Name == null)
            {
                metadata = default;
                return false;
            }

            bool successful = TryGetMetadata(metadataName.Name, out object? rawMetadata);
            if (successful)
            {
                metadata = rawMetadata is null ? default : (T)rawMetadata;
                return true;
            }

            metadata = default;
            return false;
        }

        /// <summary>
        /// Gets a list of the metadata names that are available on the lease.
        /// </summary>
        public abstract IEnumerable<string> MetadataNames { get; }

        /// <summary>
        /// Gets a list of all the metadata that is available on the lease.
        /// </summary>
        /// <returns>List of key-value pairs of metadata name and metadata object.</returns>
        public virtual IEnumerable<KeyValuePair<string, object?>> GetAllMetadata()
        {
            foreach (string name in MetadataNames)
            {
                if (TryGetMetadata(name, out object? metadata))
                {
                    yield return new KeyValuePair<string, object?>(name, metadata);
                }
            }
        }

        /// <summary>
        /// Dispose the lease. This may free up space on the limiter implementation the lease came from.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method for implementations to write.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) { }
    }
}
