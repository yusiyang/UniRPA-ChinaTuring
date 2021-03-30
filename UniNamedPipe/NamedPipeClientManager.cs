using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace UniNamedPipe
{
    public class NamedPipeClientManager
    {
        private static Dictionary<string, NamedPipeClient> _clientPool = new Dictionary<string, NamedPipeClient>();

        private static object _lockObj = new object();

        public static NamedPipeClient Create(string serverName, string pipeName, PipeDirection direction = PipeDirection.InOut, PipeOptions options = PipeOptions.None, TokenImpersonationLevel impersonationLevel = TokenImpersonationLevel.None)
        {
            var name = $"{serverName}#{pipeName}";
            if (_clientPool.ContainsKey(name))
            {
                return _clientPool[name];
            }
            var pipeClient = new NamedPipeClient(serverName,pipeName,direction,options,impersonationLevel);

            _clientPool[name] = pipeClient;

            return pipeClient;
        }

        public static void Remove(string serverName, string pipeName)
        {
            lock (_lockObj)
            {
                var name = $"{serverName}#{pipeName}";
                if (_clientPool.TryGetValue(name, out var client))
                {
                    client.Dispose();
                    _clientPool.Remove(name);
                }
            }
        }

        public static void Clear()
        {
            if (_clientPool?.Count > 0)
            {
                foreach (var client in _clientPool)
                {
                    client.Value.Dispose();
                }
                _clientPool.Clear();
            }
        }
    }
}
