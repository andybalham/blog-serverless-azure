using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Utilities;

public class EmbeddedResourceReader
{
    public static string ReadEmbeddedFile(string resourceName)
    {
        // Get the current assembly
        Assembly assembly = Assembly.GetExecutingAssembly();

        // Read the resource stream from the assembly
        var stream = 
            assembly.GetManifestResourceStream(resourceName) 
            ?? throw new InvalidOperationException("Could not find embedded resource");

        using StreamReader reader = new(stream);

        return reader.ReadToEnd();
    }
}
