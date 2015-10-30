// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Dashboard.Data
{
    public class VersionedMetadata
    {
        public VersionedMetadata(string id, string eTag, IDictionary<string, string> metadata, DateTimeOffset version)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            else if (eTag == null)
            {
                throw new ArgumentNullException(nameof(eTag));
            }
            else if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            Id = id;
            ETag = eTag;
            Metadata = metadata;
            Version = version;
        }

        public string Id { get; private set; }
        public string ETag { get; private set; }
        public IDictionary<string, string> Metadata { get; private set; }
        public DateTimeOffset Version { get; private set; }
    }
}
