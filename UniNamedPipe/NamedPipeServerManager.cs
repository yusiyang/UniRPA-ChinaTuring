using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniNamedPipe
{
    public class NamedPipeServerManager
    {
        private static Dictionary<string, NamedPipeServer> _serverPool = new Dictionary<string, NamedPipeServer>();

        private static object _lockObj = new object();

        public static NamedPipeServer Create(string name, PipeDirection pipeDirection = PipeDirection.InOut, int maxNumberOfServerInstances = 2)
        {
            lock (_lockObj)
            {
                if (!_serverPool.TryGetValue(name,out var pipeServer))
                {
                    pipeServer = new NamedPipeServer(name, pipeDirection, maxNumberOfServerInstances);
                    _serverPool.Add(name, pipeServer);
                }

                return pipeServer;
            }
        }

        public static void Remove(string name)
        {
            lock (_lockObj)
            {
                if (_serverPool.TryGetValue(name, out var pipeServer))
                {
                    pipeServer.Dispose();
                    _serverPool.Remove(name);
                }
            }
        }

        public static void Clear()
        {
            lock (_lockObj)
            {
                if(_serverPool?.Count>0)
                {
                    foreach(var server in _serverPool)
                    {
                        server.Value.Dispose();
                    }
                    _serverPool.Clear();
                }
            }
        }
    }
}
