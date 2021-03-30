using System;
using System.Diagnostics;
using System.IO.Pipes;

namespace Plugins.Shared.Library.UiAutomation.Browser.NamedPipe
{
    /// <summary>
    /// Provides client functionality for named pipe communication.
    /// </summary>
    public class NamedPipeClient : NamedPipeEndpointBase<NamedPipeClientStream>
    {
        private const string LocalComputer = ".";

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeClient"/> class.
        /// </summary>
        /// <param name="pipeName">Name of the pipe.</param>
        public NamedPipeClient(string pipeName)
            : base(pipeName)
        {

        }

        /// <summary>
        /// Gets a value indicating whether auto-flush should be activated on the
        /// pipe writer.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if auto-flush should be activated on the pipe writer;
        ///		otherwise, <c>false</c>.
        /// </value>
        protected override bool AutoFlushPipeWriter
        {
            get { return true; }
        }

        /// <summary>
        /// Creates the <see cref="PipeStream"/> to be represented by the current instance.
        /// </summary>
        /// <returns>The <see cref="PipeStream"/> to be represented by the current instance.</returns>
        protected override NamedPipeClientStream CreateStream()
        {
            var stream = new NamedPipeClientStream(
                serverName: LocalComputer,
                pipeName: PipeName,
                direction: PipeDirection.InOut,
                options: PipeOptions.Asynchronous);
            stream.Connect();
            stream.ReadMode = PipeTransmissionMode.Message;

            return stream;
        }

        /// <summary>
        /// Reads data from the pipe.
        /// </summary>
        protected override void ReadFromPipe()
        {
            try
            {
                while (Pipe != null && !StopRequested)
                {
                    if (Pipe.IsConnected)
                    {
                        string message = ReadMessage();
                        OnMessageReceived(new MessageReceivedEventArgs(message));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                Debug.WriteLine(@"Client shutting down.");
            }
        }
    }
}