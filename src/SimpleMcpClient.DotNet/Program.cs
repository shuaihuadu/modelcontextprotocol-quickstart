using Microsoft.Extensions.Configuration;

namespace SimpleMcpClient;

class Program
{
    static async Task Main(string[] args)
    {
        IConfigurationRoot configRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("mcp_server_config.json")
            .Build();

        // 绑定到自定义类型
        McpServerConfigRoot mcpConfig = new();
        configRoot.Bind(mcpConfig);

        try
        {
            var llmClient = new LLMClient();
            var chatSession = new ChatSession(llmClient);
            await chatSession.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程序异常: {ex.Message}");
        }
    }
}