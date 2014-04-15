using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace DevelopmentWithADot.AspNetSignalRVideoStreaming
{
	class VideoStreamingHub : PersistentConnection
	{
		protected override Task OnReceived(IRequest request, String connectionId, String data)
		{
			return (this.Connection.Broadcast(data));
		}
	}
}