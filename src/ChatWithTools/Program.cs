﻿using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using OpenAI;

// Connect to an MCP server
Console.WriteLine("Connecting client to MCP 'everything' server");
var mcpClient = await McpClientFactory.CreateAsync(
    new StdioClientTransport(new()
    {
        Command = "npx",
        Arguments = ["-y", "--verbose", "@modelcontextprotocol/server-everything"],
        Name = "Everything",
    }));

// Get all available tools
Console.WriteLine("Tools available:");
var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"  {tool}");
}
Console.WriteLine();

using IChatClient chatClient =
    new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")).GetChatClient("gpt-4o-mini").AsIChatClient()
    .AsBuilder().UseFunctionInvocation().Build();

List<ChatMessage> messages = [];

while (true)
{
    Console.Write("Q: ");
    messages.Add(new(ChatRole.User, Console.ReadLine()));

    List<ChatResponseUpdate> updates = [];
    await foreach (var update in chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))
    {
        Console.Write(update);
        updates.Add(update);
    }
    Console.WriteLine();

    messages.AddMessages(updates);
}