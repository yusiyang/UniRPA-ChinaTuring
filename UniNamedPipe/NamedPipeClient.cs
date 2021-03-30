using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniNamedPipe.Events;
using UniNamedPipe.Exceptions;
using UniNamedPipe.Models;
using UniNamedPipe.Utils;

namespace UniNamedPipe
{
    public class NamedPipeClient:IDisposable
    {
        private NamedPipeClientStream _clientStream;

        private object _lockObj = new object();

        private Timer _timer;

        private bool _isTimerBegin = false;

        public event PipeServerEndEventHandler PipeServerEnd;

        public NamedPipeClient(NamedPipeClientStream clientStream)
        {
            _clientStream = clientStream;

            _timer = new Timer(CheckIsRunning);
        }

        public NamedPipeClient(string serverName, string pipeName, PipeDirection direction=PipeDirection.InOut, PipeOptions options=PipeOptions.None, TokenImpersonationLevel impersonationLevel= TokenImpersonationLevel.None)
        {
            _clientStream = new NamedPipeClientStream(serverName, pipeName, direction,options,impersonationLevel);

            _timer = new Timer(CheckIsRunning);
        }

        ~NamedPipeClient()
        {
            Dispose();
        }

        public void Connect()
        {
            if (_clientStream != null && !_clientStream.IsConnected)
            {
                _clientStream.Connect(10000);
                if (!_isTimerBegin)
                {
                    StartTimer();
                    _isTimerBegin = true;
                }
            }
        }

        public Response Send(Request request)
        {
            try
            {
                lock (_lockObj)
                {
                    Connect();

                    _clientStream.WriteObject(request);
                    var response = _clientStream?.ReadObject<Response>();
                    return response;
                }
            }
            catch(Exception ex)
            {
                return Response.Fail(request,ex.Message);
            }
        }

        #region timer
        /// <summary>
        /// 计时器回调
        /// </summary>
        /// <param name="state"></param>
        private void CheckIsRunning(object state)
        {
            StopTimer();
            var request = new Request("MonitorServer", "IsRunning", null);
            var response = Send(request);
            if (response == null || !response.IsSuccess)
            {
                PipeServerEnd?.Invoke(null, new PipeServerEndEventArgs());
            }
            else
            {
                StartTimer();
            }
        }

        /// <summary>
        /// 开始执行
        /// </summary>
        public void StartTimer()
        {
            _timer.Change(200, 0);
        }

        /// <summary>
        /// 停止执行
        /// </summary>
        public void StopTimer()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        #endregion

        public void Dispose()
        {
            _clientStream?.Close();
            _clientStream = null;
            GC.SuppressFinalize(this);
        }
    }
}
