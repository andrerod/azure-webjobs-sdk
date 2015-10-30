﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Dashboard.Data
{
    public class FunctionIndexEntry
    {
        private const string IdKey = "Id";
        private const string ShortNameKey = "ShortName";
        private const string FullNameKey = "FullName";
        private const string HeartbeatExpirationInSecondsKey = "HeartbeatExpirationInSeconds";
        private const string HeartbeatSharedContainerNameKey = "HeartbeatSharedContainerName";
        private const string HeartbeatSharedDirectoryNameKey = "HeartbeatSharedDirectoryName";

        private readonly int? _heartbeatExpirationInSeconds;
        private readonly string _heartbeatSharedContainerName;
        private readonly string _heartbeatSharedDirectoryName;

        private FunctionIndexEntry(DateTimeOffset hostVersion, string id, string fullName, string shortName,
            int? heartbeatExpirationInSeconds, string heartbeatSharedContainerName, string heartbeatSharedDirectoryName)
        {
            HostVersion = hostVersion;
            Id = id;
            FullName = fullName;
            ShortName = shortName;
            _heartbeatExpirationInSeconds = heartbeatExpirationInSeconds;
            _heartbeatSharedContainerName = heartbeatSharedContainerName;
            _heartbeatSharedDirectoryName = heartbeatSharedDirectoryName;
        }

        public DateTimeOffset HostVersion { get; private set; }

        public string Id { get; private set; }

        public string FullName { get; private set; }

        public string ShortName { get; private set; }

        public int? HeartbeatExpirationInSeconds 
        { 
            get 
            { 
                return _heartbeatExpirationInSeconds; 
            } 
        }

        public string HeartbeatSharedContainerName 
        { 
            get 
            { 
                return _heartbeatSharedContainerName; 
            } 
        }

        public string HeartbeatSharedDirectoryName 
        { 
            get 
            { 
                return _heartbeatSharedDirectoryName; 
            } 
        }

        public static FunctionIndexEntry Create(IDictionary<string, string> metadata, DateTimeOffset version)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            string id = GetMetadataString(metadata, IdKey);
            string fullName = GetMetadataString(metadata, FullNameKey);
            string shortName = GetMetadataString(metadata, ShortNameKey);
            int? heartbeatExpirationInSeconds = GetMetadataNullableInt32(metadata, HeartbeatExpirationInSecondsKey);
            string heartbeatSharedContainerName = GetMetadataString(metadata, HeartbeatSharedContainerNameKey);
            string heartbeatSharedDirectoryName = GetMetadataString(metadata, HeartbeatSharedDirectoryNameKey);

            return new FunctionIndexEntry(version, id, fullName, shortName, heartbeatExpirationInSeconds,
                heartbeatSharedContainerName, heartbeatSharedDirectoryName);
        }

        public static IDictionary<string, string> CreateOtherMetadata(FunctionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            Dictionary<string, string> metadata = new Dictionary<string, string>();
            AddMetadataString(metadata, IdKey, snapshot.Id);
            AddMetadataString(metadata, FullNameKey, snapshot.FullName);
            AddMetadataString(metadata, ShortNameKey, snapshot.ShortName);
            AddMetadataNullableInt32(metadata, HeartbeatExpirationInSecondsKey, snapshot.HeartbeatExpirationInSeconds);
            AddMetadataString(metadata, HeartbeatSharedContainerNameKey, snapshot.HeartbeatSharedContainerName);
            AddMetadataString(metadata, HeartbeatSharedDirectoryNameKey, snapshot.HeartbeatSharedDirectoryName);

            return metadata;
        }

        private static void AddMetadataString(IDictionary<string, string> metadata, string key, string value)
        {
            if (value != null)
            {
                metadata.Add(key, value);
            }
        }

        private static void AddMetadataNullableInt32(IDictionary<string, string> metadata, string key, int? value)
        {
            if (value.HasValue)
            {
                metadata.Add(key, value.Value.ToString("g", CultureInfo.InvariantCulture));
            }
        }

        private static string GetMetadataString(IDictionary<string, string> metadata, string key)
        {
            Debug.Assert(metadata != null);

            if (!metadata.ContainsKey(key))
            {
                return null;
            }

            return metadata[key];
        }

        private static int? GetMetadataNullableInt32(IDictionary<string, string> metadata, string key)
        {
            Debug.Assert(metadata != null);

            if (!metadata.ContainsKey(key))
            {
                return null;
            }

            string unparsed = metadata[key];
            int parsed;

            if (!Int32.TryParse(unparsed, NumberStyles.None, CultureInfo.InvariantCulture, out parsed))
            {
                return null;
            }

            return parsed;
        }
    }
}
