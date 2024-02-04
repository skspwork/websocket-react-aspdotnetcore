using Microsoft.AspNetCore.Mvc;

namespace websocket_react_aspdotnetcore.Controllers;

[ApiController]
[Route("[controller]")]
public class LogController : ControllerBase
{
    [Route("/ws/TrailLog")]
    public async Task TrailLog(string fileName)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return;

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        using var logStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var logStreamReader = new StreamReader(logStream);

        bool open = true;
        try
        {
            _ = Task.Run(async () =>
            {
                while (open)
                {
                    var line = await logStreamReader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line))
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    await webSocket.SendAsync(new LogResponse()
                    {
                        Log = line + "\n"
                    });
                }
            });
            while (true)
            {
                var request = await webSocket.ReadAsync<LogRequest>();
                if (request.IsClosed || request.Data == null)
                    break;

                if (request.Data.LogCommand == LogCommand.Ping)
                    continue;
            }
        }
        finally
        {
            open = true;
        }
    }
}

public enum LogCommand
{
    Ping = 1,
}
public class LogRequest
{
    public LogCommand LogCommand { get; set; }
}

public class LogResponse
{
    public string Log { get; set; }
}
