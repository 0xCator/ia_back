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

        public async Task Start(System.Net.WebSockets.WebSocket socket)
        {
            this.socket = socket;
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            id = int.Parse(message);
        }


    }
}
