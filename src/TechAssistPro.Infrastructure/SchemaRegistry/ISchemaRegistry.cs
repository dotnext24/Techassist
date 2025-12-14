using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NJsonSchema;

namespace TechAssistPro.Infrastructure.SchemaRegistry
{
   public interface ISchemaRegistry
{
    Task<JsonSchema> GetSchemaAsync(string eventType, int version);
    Task RegisterSchemaAsync(string eventType, int version, JsonSchema schema);
    Task RegisterSchemaFromFileAsync(string eventType, int version, string filePath);
    Task<bool> ValidateAsync(string eventType, int version, string jsonPayload);
}
}