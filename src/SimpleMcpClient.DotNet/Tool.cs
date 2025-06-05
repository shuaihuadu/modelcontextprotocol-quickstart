using System.Text;
using System.Text.Json;

namespace SimpleMcpClient;

public class Tool
{
    public string Name { get; }
    public string Description { get; }
    public JsonElement InputSchema { get; }

    public Tool(string name, string description, JsonElement inputSchema)
    {
        this.Name = name;
        this.Description = description;
        this.InputSchema = inputSchema;
    }

    /// <summary>
    /// 格式化工具信息供LLM使用
    /// </summary>
    public string FormatForLlm()
    {
        var argsDesc = new StringBuilder();

        IReadOnlyList<string> parameterMetas = MapParameterMetadata(InputSchema);

        return $@"
工具: {Name}
描述: {Description}
参数:
{string.Join(Environment.NewLine, parameterMetas)}";
    }

    private static IReadOnlyList<string> MapParameterMetadata(JsonElement schema)
    {
        List<string> metadata = [];

        if (!schema.TryGetProperty("properties", out JsonElement properties))
        {
            return [];
        }

        string parameterFormat = "- {0}: {1} {2}";

        HashSet<string>? requiredParameters = GetRequiredParameterNames(schema);

        foreach (var param in properties.EnumerateObject())
        {
            string? paramDescription = param.Value.TryGetProperty("description", out JsonElement description) ? description.GetString() : null;
            bool isRequired = requiredParameters?.Contains(param.Name) ?? false;

            metadata.Add(string.Format(parameterFormat, param.Name, paramDescription, isRequired ? "(Required)" : ""));
        }

        return metadata;
    }

    private static HashSet<string>? GetRequiredParameterNames(JsonElement schema)
    {
        HashSet<string>? requiredParameterNames = null;

        if (schema.TryGetProperty("required", out JsonElement requiredElement) && requiredElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var node in requiredElement.EnumerateArray())
            {
                requiredParameterNames ??= [];
                requiredParameterNames.Add(node.GetString()!);
            }
        }

        return requiredParameterNames;
    }
}