﻿using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace AspNetCoreSseServer.Tools;

[McpServerToolType]
public sealed class SampleLlmTool
{
    [McpServerTool(Name = "sampleLLM"), Description("Samples from an LLM using MCP's sampling feature")]
    public static async Task<string> SampleLLM(
        IMcpServer thisServer,
        [Description("The prompt to send to the LLM")] string prompt,
        [Description("Maximum number of tokens to generate")] int maxTokens,
        CancellationToken cancellationToken)
    {
        ChatMessage[] messages =
        [
            new(ChatRole.System,"You are a helpful test server."),
            new(ChatRole.User,prompt)
        ];

        ChatOptions options = new()
        {
            MaxOutputTokens = maxTokens,
            Temperature = 0.7f
        };

        ChatResponse samplingResponse = await thisServer.AsSamplingChatClient().GetResponseAsync(messages, options, cancellationToken);

        return $"LLM sampling result: {samplingResponse}";
    }
}
