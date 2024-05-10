using System.Text;
using System.Net.WebSockets;
using ia_back.Models;

namespace ia_back.WebSocket
{
    class SocketManager
    {
        private static SocketManager _instance;
        private static readonly object _lock = new object();
        private List<Socket> _sockets = new List<Socket>();

        public static SocketManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SocketManager();
                    }
                    return _instance;
                }
            }
        }

        public void AddSocket(Socket socket)
        {
            _sockets.Add(socket);
        }

        public void RemoveSocket(Socket socket)
        {
            _sockets.Remove(socket);
        }

        public async Task ProjectHasUpdate(int userId)
        {
            
            var message = Encoding.UTF8.GetBytes("Project has been updated");
            foreach (var socket in _sockets)
            {
                if (socket.id == userId)
                {
                    await socket.socket.SendAsync(new ArraySegment<byte>(message, 0, message.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public async Task ProjectHasUpdate(ICollection<User> users)
        {
            var message = Encoding.UTF8.GetBytes("Project has been updated");
            foreach (var socket in _sockets)
            {
                foreach (var user in users)
                {
                    if (socket.id == user.Id)
                    {
                        await socket.socket.SendAsync(new ArraySegment<byte>(message, 0, message.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }

        public async Task TaskHasUpdate(ICollection<User> users)
        {
            var message = Encoding.UTF8.GetBytes("Task has been updated");
            foreach (var socket in _sockets)
            {
                foreach (var user in users)
                {
                    if (socket.id == user.Id)
                    {
                        await socket.socket.SendAsync(new ArraySegment<byte>(message, 0, message.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }
    }
}
