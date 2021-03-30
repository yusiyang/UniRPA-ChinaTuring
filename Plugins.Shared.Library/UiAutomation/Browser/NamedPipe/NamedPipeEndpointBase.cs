using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace Plugins.Shared.Library.UiAutomation.Browser.NamedPipe
{
    /// <summary>
    /// Base class for functionality common to both server and client.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="PipeStream"/> to represent.</typeparam>
    public abstract class NamedPipeEndpointBase<T> : INamedPipeEndpoint where T : PipeStream
    {
        /// <summary>The size of the buffer for reads and writes.</summary>
        protected const int BufferSize = 4096;

        /// <summary>The stream writer to use when writing messages.</summary>
        private StreamWriter pipeWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeEndpointBase&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="pipeName">Name of the pipe.</param>
        protected NamedPipeEndpointBase(string pipeName)
        {
            if (pipeName == null)
            {
                throw new ArgumentNullException("pipeName");
            }
            PipeName = pipeName;
        }

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Gets the name of the pipe.
        /// </summary>
        /// <value>
        /// The name of the pipe.
        /// </value>
        public string PipeName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the Stop method has been called.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Stop method has been called; otherwise, <c>false</c>.
        /// </value>
        protected bool StopRequested { get; private set; }

        /// <summary>
        /// Gets the <see cref="PipeStream"/> represented by the current instance.
        /// </summary>
        protected T Pipe { get; private set; }

        /// <summary>
        /// Gets a value indicating whether auto-flush should be activated on the
        /// pipe writer.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if auto-flush should be activated on the pipe writer;
        ///		otherwise, <c>false</c>.
        /// </value>
        protected abstract bool AutoFlushPipeWriter { get; }

        /// <summary>
        /// Starts listening for incoming messages.
        /// </summary>
        public void Start()
        {
            Pipe = CreateStream();
            ThreadPool.QueueUserWorkItem(state => ReadFromPipe());
        }

        /// <summary>
        /// Stops listening for incoming messages.
        /// </summary>
        public void Stop()
        {
            StopRequested = true;
            Pipe.Close();
            Pipe.Dispose();
        }

        /// <summary>
        /// Writes the specified message to the pipe.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Write(string message)
        {
            if (Pipe.IsConnected && Pipe.CanWrite)
            {
                if (pipeWriter == null)
                {
                    pipeWriter = new StreamWriter(Pipe)
                    {
                        AutoFlush = AutoFlushPipeWriter
                    };
                }

                pipeWriter.WriteLine(message);
            }
        }

        /// <summary>
        /// Creates the <see cref="PipeStream"/> to be represented by the current instance.
        /// </summary>
        /// <returns>The <see cref="PipeStream"/> to be represented by the current instance.</returns>
        protected abstract T CreateStream();

        /// <summary>
        /// Reads data from the pipe.
        /// </summary>
        protected abstract void ReadFromPipe();

        /// <summary>
        /// Raises the <see cref="E:MessageReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="MessageReceivedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, e);
            }
        }

        /// <summary>
        /// Reads an incoming message from the pipe.
        /// </summary>
        /// <returns>The received message.</returns>
        protected string ReadMessage()
        {
            var memoryStream = new MemoryStream();
            var buffer = new byte[BufferSize];

            do
            {
                int bytesRead = Pipe.Read(buffer, 0, buffer.Length);
                memoryStream.Write(buffer, 0, bytesRead);
            } while (!Pipe.IsMessageComplete);

            return Encoding.Default.GetString(memoryStream.ToArray());
        }
    }
}