using System;

namespace Plugins.Shared.Library.UiAutomation.Browser.NamedPipe
{
	/// <summary>
	/// Provides data for <see cref="INamedPipeEndpoint.MessageReceived"/> events.
	/// </summary>
	public class MessageReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the received message.
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageReceivedEventArgs"/> class.
		/// </summary>
		/// <param name="message">The received message.</param>
		public MessageReceivedEventArgs(string message)
		{
			Message = message;
		}
	}
}