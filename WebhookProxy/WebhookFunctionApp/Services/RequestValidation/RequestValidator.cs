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

        public RequestValidationResult Validate(string contractId, string requestBody)
        {
            var contractSchemaJson = EmbeddedResourceReader.ReadEmbeddedFile($"WebhookFunctionApp.Schemas.{contractId}.json");
            var contractSchema = JSchema.Parse(contractSchemaJson);

            var requestBodyJsonObject = JObject.Parse(requestBody);

            bool isValid = requestBodyJsonObject.IsValid(contractSchema, out IList<string> errorMessages);

            return new RequestValidationResult { IsValid = isValid, ErrorMessages = errorMessages };
        }
    }
}
