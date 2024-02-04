using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
namespace websocket_react_aspdotnetcore;

public static class WebSocketExtension
{
    public static async Task SendAsync<T>(this WebSocket self, T obj)
    {
        await self.SendAsync(
            new ArraySegment<byte>(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(obj))),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public static async Task<WebSocketData<T>> ReadAsync<T>(this WebSocket self) where T : class
    {
        var buffer = new byte[1024];
        var receivedResult = await self.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (receivedResult.Count == 0)
            return new WebSocketData<T>
            {
                IsClosed = receivedResult.CloseStatus.HasValue,
                Data = null
            };

        var read = buffer.Take(receivedResult.Count);
        var ascii = Encoding.ASCII.GetString(read.ToArray());

        var res = JsonSerializer.Deserialize<T>(ascii);
        return new WebSocketData<T>
        {
            IsClosed = receivedResult.CloseStatus.HasValue,
            Data = res
        };
    }
}