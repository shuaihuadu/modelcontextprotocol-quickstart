using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.ClientModel;

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddHttpClientInstrumentation()
    .AddSource("*")
    .AddOtlpExporter()
    .Build();

using var metricsProvider = Sdk.CreateMeterProviderBuilder()
    .AddHttpClientInstrumentation()
    .AddMeter("*")
    .AddOtlpExporter()
    .Build();

using var loggerFactory = LoggerFactory.Create(builder => builder.AddOpenTelemetry(opt => opt.AddOtlpExporter()));

IConfigurationRoot configRoot = new ConfigurationBuilder()
    .AddJsonFile(@"D:\appsettings\semantic-kernel-quickstart.json", true)
    .Build();


// Connect to an MCP server
Console.WriteLine("Connecting client to MCP 'everything' server");

string endpoint = configRoot["AzureOpenAI:Endpoint"]!;
string apiKey = configRoot["AzureOpenAI:ApiKey"]!;
string deploymentName = configRoot["AzureOpenAI:DeploymentName"]!;

AzureOpenAIClient azureOpenAIClient = new(new Uri(endpoint), new ApiKeyCredential(apiKey));

IChatClient samplingClient = azureOpenAIClient.GetChatClient(deploymentName).AsIChatClient()
    .AsBuilder()
    .UseOpenTelemetry(loggerFactory: loggerFactory, configure: o => o.EnableSensitiveData = true)
    .Build();

IMcpClient mcpClient = await McpClientFactory.CreateAsync(
    new StdioClientTransport(new()
    {
        Command = "npx",
        Arguments = ["-y", "--verbose", "@modelcontextprotocol/server-everything"],
        Name = "Everything"
    }),
    clientOptions: new()
    {
        Capabilities = new()
        {
            Sampling = new()
            {
                SamplingHandler = samplingClient.CreateSamplingHandler()
            }
        }
    }, loggerFactory: loggerFactory);


Console.WriteLine("Tools available:");

IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

foreach (McpClientTool tool in tools)
{
    Console.WriteLine($"    {tool}");
}

Console.WriteLine();

using IChatClient chatClient = azureOpenAIClient.GetChatClient(deploymentName).AsIChatClient()
    .AsBuilder()
    .UseOpenTelemetry(loggerFactory: loggerFactory, configure: o => o.EnableSensitiveData = true)
    .Build();

List<ChatMessage> messages = [];

while (true)
{
    Console.WriteLine("Q: ");
    messages.Add(new(ChatRole.User, Console.ReadLine()));

    List<ChatResponseUpdate> updates = [];

    await foreach (ChatResponseUpdate update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))
    {
        Console.Write(update);
        updates.Add(update);
    }

    Console.WriteLine();

    messages.AddMessages(updates);
}