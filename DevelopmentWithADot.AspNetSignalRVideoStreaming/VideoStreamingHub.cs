using System;
using Microsoft.AspNet.SignalR;

namespace DevelopmentWithADot.AspNetSignalRVideoStreaming
{
	public class VideoStreamingHub : Hub
	{
		public void Send(String imageUrl, Int32 imageWidth, Int32 imageHeight)
		{
			this.Clients.All.Send(imageUrl, imageWidth, imageHeight);
		}
	}
}
