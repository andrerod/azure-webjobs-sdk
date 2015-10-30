﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Azure.WebJobs.Host.Storage;

namespace Microsoft.Azure.WebJobs.Host.Bindings
{
    /// <summary>
    /// Provides context for parameter bind operations. See <see cref="IBinding"/>
    /// or <see cref="IBindingProvider"/> for more information.
    /// </summary>
    public class BindingProviderContext
    {
        private readonly ParameterInfo _parameter;
        private readonly IReadOnlyDictionary<string, Type> _bindingDataContract;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="parameter">The parameter to bind to.</param>
        /// <param name="bindingDataContract">The binding data contract.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
        public BindingProviderContext(ParameterInfo parameter, IReadOnlyDictionary<string, Type> bindingDataContract, CancellationToken cancellationToken)
        {
            _parameter = parameter;
            _bindingDataContract = bindingDataContract;
            _cancellationToken = cancellationToken;
        }
        
        /// <summary>
        /// Gets the parameter to bind to.
        /// </summary>
        public ParameterInfo Parameter
        {
            get { return _parameter; }
        }

        /// <summary>
        /// Gets the data contract for the binding.
        /// </summary>
        public IReadOnlyDictionary<string, Type> BindingDataContract
        {
            get { return _bindingDataContract; }
        }

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> to use.
        /// </summary>
        public CancellationToken CancellationToken
        {
            get { return _cancellationToken; }
        }

        /// <summary>
        /// The storage connection string.
        /// </summary>
        internal IStorageAccount StorageAccount { get; set; }
    }
}
