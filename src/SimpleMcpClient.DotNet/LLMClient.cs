using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat; // 需要安装DotNetEnv包

public class LLMClient
{
    /// <summary>
    /// 获取LLM响应
    /// </summary>
    public async Task<string> GetResponseAsync(List<ChatMessage> messages)
    {
        IConfigurationRoot configRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@"D:\appsettings\semantic-kernel-quickstart.json", true)
            .Build();

        string endpoint = configRoot["AzureOpenAI:Endpoint"]!;
        string apiKey = configRoot["AzureOpenAI:ApiKey"]!;
        string deploymentName = configRoot["AzureOpenAI:DeploymentName"]!;

        AzureOpenAIClient azureClient = new(new Uri(endpoint), new AzureKeyCredential(apiKey));

        ChatClient chatClient = azureClient.GetChatClient(deploymentName);


        ChatCompletion completion = await chatClient.CompleteChatAsync(messages);


        return completion.Content[0].Text;
    }
}