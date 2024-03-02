
namespace WebhookFunctionApp.Services.RequestValidation
{
    public partial class RequestValidator
    {
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
                return reader.ReadToEnd();
            }
        }
    }
}
