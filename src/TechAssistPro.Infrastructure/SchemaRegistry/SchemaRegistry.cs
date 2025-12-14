using NJsonSchema;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace TechAssistPro.Infrastructure.SchemaRegistry
{
   public class SchemaRegistry : ISchemaRegistry
{
    private readonly ILogger<SchemaRegistry> _logger;
    private readonly ConcurrentDictionary<string, JsonSchema> _schemas;

    public SchemaRegistry(ILogger<SchemaRegistry> logger)
    {
        _logger = logger;
        _schemas = new ConcurrentDictionary<string, JsonSchema>();
    }

    public Task<JsonSchema> GetSchemaAsync(string eventType, int version)
    {
        var key = GetKey(eventType, version);
        
        if (_schemas.TryGetValue(key, out var schema))
        {
            return Task.FromResult(schema);
        }

        throw new InvalidOperationException(
            $"Schema not found: {eventType} v{version}");
    }

    public Task RegisterSchemaAsync(string eventType, int version, JsonSchema schema)
    {
        var key = GetKey(eventType, version);
        _schemas[key] = schema;

        _logger.LogInformation(
            "Registered schema: {EventType} v{Version}",
            eventType, version);

        return Task.CompletedTask;
    }

    public async Task RegisterSchemaFromFileAsync(string eventType, int version, string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Schema file not found: {filePath}");
        }

        var schemaJson = await File.ReadAllTextAsync(filePath);
        var schema = await JsonSchema.FromJsonAsync(schemaJson);

        await RegisterSchemaAsync(eventType, version, schema);

        _logger.LogInformation(
            "Loaded schema from file: {EventType} v{Version} from {File}",
            eventType, version, filePath);
    }

    public async Task<bool> ValidateAsync(string eventType, int version, string jsonPayload)
    {
        try
        {
            var schema = await GetSchemaAsync(eventType, version);
            var errors = schema.Validate(jsonPayload);

            if (errors.Any())
            {
                _logger.LogWarning(
                    "Validation failed for {EventType} v{Version}: {Errors}",
                    eventType, version, 
                    string.Join("; ", errors.Select(e => e.ToString())));
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error validating {EventType} v{Version}",
                eventType, version);
            return false;
        }
    }

    private static string GetKey(string eventType, int version) => 
        $"{eventType}:v{version}";
}
}