using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevelopmentWithADot.AspNetSignalRVideoStreaming;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(VideoStreaming))]

namespace DevelopmentWithADot.AspNetSignalRVideoStreaming
{
	public class VideoStreaming : WebControl
	{
		public VideoStreaming() : base("video")
		{
			this.Interval = 100;
			this.OnStreamed = String.Empty;
			this.Source = false;
			this.StreamingMode = VideoStreamingMode.Event;
			this.TargetClientID = String.Empty;
			this.Url = "/videostreaming";
		}

		[DefaultValue(false)]
		public Boolean Source
		{
			get;
			set;
		}

		[DefaultValue(VideoStreamingMode.Event)]
		public VideoStreamingMode StreamingMode
		{
			get;
			set;
		}

		[DefaultValue("/videostreaming")]
		public String Url
		{
			get;
			set;
		}

		[DefaultValue(100)]
		public Int32 Interval
		{
			get;
			set;
		}

		[DefaultValue("")]
		public String OnStreamed
		{
			get;
			set;
		}

		[DefaultValue("")]
		public String TargetClientID
		{
			get;
			set;
		}

		public void Configuration(IAppBuilder app)
		{
			app.MapSignalR<VideoStreamingHub>(this.Url);
		}

		protected override void OnLoad(EventArgs e)
		{
			var sm = ScriptManager.GetCurrent(this.Page);
			var streamingAction = String.Empty;

			if (this.Interval == 0)
			{
				throw (new ArgumentNullException("Interval", "Interval cannot be 0"));				
			}

			if (String.IsNullOrWhiteSpace(this.Url) == true)
			{
				throw (new ArgumentNullException("Url", "Url cannot be empty"));
			}

			switch (this.StreamingMode)
			{
				case VideoStreamingMode.Event:
					if (String.IsNullOrWhiteSpace(this.OnStreamed) == true)
					{
						throw (new ArgumentNullException("OnStreamed", "OnStreamed cannot be empty when using streaming mode Event"));
					}
					streamingAction = String.Format("{0}(data)", this.OnStreamed);
					break;

				case VideoStreamingMode.Target:
					if (String.IsNullOrWhiteSpace(this.TargetClientID) == true)
					{
						throw (new ArgumentNullException("TargetClientID", "TargetClientID cannot be empty when using streaming mode Target"));
					}
					streamingAction = String.Format("var canvas = document.getElementById('{0}'); var ctx = canvas.getContext('2d'); var img = new Image(); img.src = data; img.onload = function() {{\n ctx.drawImage(img, 0, 0, canvas.width, canvas.height); \n}};", this.TargetClientID);
					break;

				case VideoStreamingMode.Window:
					streamingAction = String.Format("if (!window.videoWindow) {{ window.videoWindow = window.open(data, '_blank', 'height={0},width={1}'); }} else {{ window.videoWindow.location.href = data; }};", (Int32)this.Width.Value, (Int32)this.Height.Value);
					break;
			}
			
			var initScript = String.Format("\ndocument.getElementById('{0}').connection = $.connection('{1}');\n", this.ClientID, this.Url);
			var startStreamScript = String.Format("\ndocument.getElementById('{0}').startStream = function(){{\n var video = document.getElementById('{0}'); video.connection.received(function(data) {{\n {2} \n}}); video.connection.start().done(function() {{\n if ((true == {3}) && (video.paused == true) && (video.src == '')) {{\n navigator.getUserMedia = (navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia); navigator.getUserMedia({{ video: true, audio: false }}, function (stream) {{\n video.src = window.URL.createObjectURL(stream); \n}}, function (error) {{\n debugger; \n}}); \n}}; if (video.intervalId) {{\n window.clearInterval(video.intervalId); \n}}; video.intervalId = window.setInterval(function() {{\n var canvas = document.createElement('canvas'); var context = canvas.getContext('2d'); context.drawImage(video, 0, 0, canvas.width, canvas.height); var picture = canvas.toDataURL(); video.connection.send(picture); \n}}, {1}); video.play(); \n}}) }}\n", this.ClientID, this.Interval, streamingAction, this.Source.ToString().ToLower());
			var stopStreamScript = String.Format("\ndocument.getElementById('{0}').stopStream = function(){{ var video = document.getElementById('{0}'); video.connection.stop(); if (video.intervalId) {{ window.clearInterval(video.intervalId); }}; video.intervalId = 0; video.pause(); }};\n", this.ClientID);
			var script = String.Concat(initScript, startStreamScript, stopStreamScript);

			if (sm != null)
			{
				this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat("videostream", this.ClientID), String.Format("Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function() {{ {0} }});\n", script), true);
			}
			else
			{
				this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat("videostream", this.ClientID), script, true);
			}

			if (this.Width != Unit.Empty)
			{
				this.Attributes.Add(HtmlTextWriterAttribute.Width.ToString().ToLower(), this.Width.ToString());
			}

			if (this.Height != Unit.Empty)
			{
				this.Attributes.Add(HtmlTextWriterAttribute.Height.ToString().ToLower(), this.Height.ToString());
			}

			this.Attributes.Remove("autoplay");
			this.Attributes.Remove("controls");
			this.Attributes.Remove("crossorigin");
			this.Attributes.Remove("loop");
			this.Attributes.Remove("mediagroup");
			this.Attributes.Remove("muted");
			this.Attributes.Remove("poster");
			this.Attributes.Remove("preload");
			this.Attributes.Remove("src");

			base.OnLoad(e);
		}		
	}
}
