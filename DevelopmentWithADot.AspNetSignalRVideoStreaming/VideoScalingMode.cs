using System;

namespace DevelopmentWithADot.AspNetSignalRVideoStreaming
{
	[Serializable]
	public enum VideoScalingMode
	{
		/// <summary>
		/// No scaling is performed.
		/// </summary>
		None,

		/// <summary>
		/// Use the original size, coming from getUserMedia.
		/// </summary>
		OriginalSize,

		/// <summary>
		/// Use the target CANVAS size.
		/// </summary>
		TargetSize,

		/// <summary>
		/// Use the VideoStreaming control size.
		/// </summary>
		ControlSize
	}
}
