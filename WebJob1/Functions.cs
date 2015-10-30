using System.IO;
using Microsoft.Azure.WebJobs;

namespace WebJob1
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        [StorageAccount(Path = "D:\\Sources\\connectionStrings.txt")]
        public static void ProcessBlobMessage([BlobTrigger("uploads/{name}")] TextReader input,
        [Blob("output/{name}")] out string output)
        {
            output = input.ReadToEnd();
        }
    }
}
