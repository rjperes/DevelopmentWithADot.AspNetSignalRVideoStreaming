using System;
using System.Web.UI;
using DevelopmentWithADot.AspNetSignalRVideoStreaming;

namespace DevelopmentWithADot.AspNetSignalRVideoStreaming.Test
{
	public partial class Default : Page
	{
		protected override void OnInit(EventArgs e)
		{
			var source = false;

			Boolean.TryParse(this.Request.QueryString["Source"], out source);

			this.video.Source = source;

			if (source == false)
			{
				this.video.Style[HtmlTextWriterStyle.Display] = "none";
			}
			else
			{
				//this.video.StreamingMode = VideoStreamingMode.None;
			}

			base.OnInit(e);
		}
	}
}