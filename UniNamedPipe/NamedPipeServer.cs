using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniNamedPipe.Models;
using UniNamedPipe.Utils;

namespace UniNamedPipe
{
    public class NamedPipeServer : IDisposable
    {
        private NamedPipeServerStream _serverStream;

        private string _name;
        private Thread _thread;
        private bool _disposed;

        public NamedPipeServer(NamedPipeServerStream serverStream)
        {
            _serverStream = serverStream;
        }

        public NamedPipeServer(string name, PipeDirection pipeDirection = PipeDirection.InOut, int maxNumberOfServerInstances = 1)
        {
            _serverStream = new NamedPipeServerStream(name, pipeDirection, maxNumberOfServerInstances);

            _name = name;
            Listen();
        }

        ~NamedPipeServer()
        {
            Dispose();
        }

        public void Connect()
        {
            if (_serverStream != null && !_serverStream.IsConnected)
            {
                _serverStream.WaitForConnection();
            }
        }

        public void Listen()
        {
            _thread = new Thread(Reply);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Reply()
        {
            try
            {
                while (!_disposed)
                {
                    Connect();

                    var request = _serverStream?.ReadObject<Request>();
                    if (request == null)
                    {
                        _serverStream?.Disconnect();
                        continue;
                    }
                    #region 处理MonitorServer

                    if (request.ApiName == "MonitorServer" && request.MethodName == "IsRunning")
                    {
                        _serverStream.WriteObject(Response.Success(request, ""));
                        continue;
                    }

                    #endregion

                    var requestHandler = new RequestHandler(_name, request);
                    var response = requestHandler.Handle();
                    _serverStream.WriteObject(response);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void Dispose()
        {
            _disposed = true;
            if (_serverStream != null)
            {
                if (_serverStream.IsConnected)
                {
                    _serverStream.Disconnect();
                }
                _serverStream.Close();
            }
            _serverStream = null;
            //_thread?.Abort();
            GC.SuppressFinalize(this);
        }
    }
}
