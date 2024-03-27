namespace WebhookFunctionApp.Utilities;

public class StreamToStringConverter
{
    public static string ConvertStreamToString(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        // Reset the stream position to the beginning (if the stream supports seeking)
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        using StreamReader reader = new(stream);

        //var memoryStream = new MemoryStream();
        //stream.CopyTo(memoryStream);

        //memoryStream.Position = 0; // Reset to the beginning before each read

        //using StreamReader reader = new(memoryStream);

        return reader.ReadToEnd();
    }
}
