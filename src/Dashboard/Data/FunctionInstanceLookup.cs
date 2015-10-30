﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Dashboard.Data
{
    internal class FunctionInstanceLookup : IFunctionInstanceLookup
    {
        private static readonly JsonSerializerSettings SerializerSettings =
            JsonConcurrentDocumentStore<FunctionInstanceSnapshot>.JsonSerializerSettings;

        private readonly CloudBlobDirectory _directory;

        public FunctionInstanceLookup(CloudBlobClient client)
            : this(client.GetContainerReference(DashboardContainerNames.Dashboard)
            .GetDirectoryReference(DashboardDirectoryNames.FunctionInstances))
        {
        }

        public FunctionInstanceLookup(CloudBlobDirectory directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            _directory = directory;
        }

        FunctionInstanceSnapshot IFunctionInstanceLookup.Lookup(Guid id)
        {
            CloudBlockBlob blob = _directory.GetBlockBlobReference(id.ToString("N"));
            string contents;

            try
            {
                contents = blob.DownloadText();
            }
            catch (StorageException exception)
            {
                if (exception.IsNotFound())
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return JsonConvert.DeserializeObject<FunctionInstanceSnapshot>(contents, SerializerSettings);
        }
   }
}
