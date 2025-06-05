using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace EverythingServer.Tools;

[McpServerToolType]
public class LongRunningTool
{
    public static async Task<string> LongRunningOperation(IMcpServer server, RequestContext<CallToolRequestParams> context, int duration = 10, int steps = 5)
    {
        var progressToken = context.Params?.Meta?.ProgressToken;
        var stepDuration = duration / steps;

        for (var i = 0; i < steps; i++)
        {
            await Task.Delay(stepDuration * 1000);

            if (progressToken is not null)
            {
                await server.SendNotificationAsync("notifications/progress", new
                {
                    Progress = i,
                    Total = steps,
                    progressToken
                });
            }
        }

        return $"Long running operation completed. Duration: {duration} seconds. Steps: {steps}.";
    }
}
