using System;

namespace DevelopmentWithADot.AspNetSignalRVideoStreaming
{
	/// <summary>
	/// The streaming modes.
	/// </summary>
	[Serializable]
	public enum VideoStreamingMode
	{
		/// <summary>
		/// No streaming.
		/// </summary>
		None,

		/// <summary>
		/// Raises a JavaScript event.
		/// </summary>
		Event,

		/// <summary>
		/// Draws directly to a target.
		/// </summary>
		Target,

		/// <summary>
		/// Draws in a new window.
		/// </summary>
		Window
	}
}
