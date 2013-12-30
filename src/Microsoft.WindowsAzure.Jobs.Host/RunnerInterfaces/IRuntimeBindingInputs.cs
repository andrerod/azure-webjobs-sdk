﻿using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Jobs
{
    // This is the basic infomration that a static binding can use to create a runtime binding.
    // There are auxillary interfaces (ITrigger*) which provide additional information specific to certain binding triggers.
    internal interface IRuntimeBindingInputs
    {
        IDictionary<string, string> NameParameters { get; } 
        string AccountConnectionString { get; }
        string ReadFile(string filename); // $$$ Throw or null on missing file?
    }
}