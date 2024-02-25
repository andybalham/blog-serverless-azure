using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebhookFunctionApp.Utilities;

namespace WebhookFunctionApp.Services.RequestValidation
{
    public partial class RequestValidator : IRequestValidator
    {

        public RequestValidationResult Validate(string contractId, Stream requestBodyStream)
        {
            var contractSchemaJson = EmbeddedResourceReader.ReadEmbeddedFile($"WebhookFunctionApp.Schemas.{contractId}.json");
            var contractSchema = JSchema.Parse(contractSchemaJson);

            var requestBodyJson = StreamToStringConverter.ConvertStreamToString(requestBodyStream);
            var requestBodyJsonObject = JObject.Parse(requestBodyJson);

            bool isValid = requestBodyJsonObject.IsValid(contractSchema, out IList<string> errorMessages);

            return new RequestValidationResult { IsValid = isValid, ErrorMessages = errorMessages };
        }
    }
}
