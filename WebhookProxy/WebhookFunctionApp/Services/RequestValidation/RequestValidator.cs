using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebhookFunctionApp.Models;
using WebhookFunctionApp.Utilities;

namespace WebhookFunctionApp.Services.RequestValidation;

public class RequestValidator : IRequestValidator
{
    public RequestValidationResult Validate(string contractId, string requestBody)
    {
        var contractSchemaJson = 
            EmbeddedResourceReader.ReadEmbeddedFile(
                $"WebhookFunctionApp.Schemas.{contractId}.json");

        var contractSchema = JSchema.Parse(contractSchemaJson);

        JObject requestBodyJsonObject;
        try
		{
            requestBodyJsonObject = JObject.Parse(requestBody);
		}
		catch (JsonReaderException ex)
		{
            return 
                new RequestValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessages = [$"Exception when parsing JSON: {ex.Message}"],
                };
		}

        bool isValid = 
            requestBodyJsonObject.IsValid(contractSchema, out IList<string> errorMessages);

        return 
            new RequestValidationResult { IsValid = isValid, ErrorMessages = errorMessages };
    }
}
