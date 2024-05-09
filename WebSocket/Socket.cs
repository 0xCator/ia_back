using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets;


namespace ia_back.WebSocket
{
    public class Socket
    {
        public System.Net.WebSockets.WebSocket socket;
        public int id;

        public Socket(int id)
        {
            this.id = id;
            Console.WriteLine("Socket started with id: " + id);
        }

        public async Task Start(System.Net.WebSockets.WebSocket socket)
        {
            this.socket = socket;
            var buffer = new byte[1024 * 4];
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        SocketManager.Instance.RemoveSocket(this);
                        Console.WriteLine("Socket closed with id: " + id);
                    }
                    else
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    }
                }
            }
            catch (WebSocketException)
            {
            
            }
        }
    }
}
