using System;

namespace Plugins.Shared.Library.UiAutomation.Browser.NamedPipe
{
	/// <summary>
	/// Represents and endpoint in the client/server model.
	/// </summary>
	public interface INamedPipeEndpoint
	{
		/// <summary>
		/// Occurs when a message is received.
		/// </summary>
		event EventHandler<MessageReceivedEventArgs> MessageReceived;

		/// <summary>
		/// Gets the name of the pipe.
		/// </summary>
		/// <value>
		/// The name of the pipe.
		/// </value>
		string PipeName { get; }

		/// <summary>
		/// Starts listening for incoming messages.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops listening for incoming messages.
		/// </summary>
		void Stop();

		/// <summary>
		/// Writes the specified message to the pipe.
		/// </summary>
		/// <param name="message">The message to write.</param>
		void Write(string message);
	}
}