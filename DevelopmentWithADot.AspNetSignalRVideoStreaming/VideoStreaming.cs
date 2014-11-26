using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(DevelopmentWithADot.AspNetSignalRVideoStreaming.VideoStreaming))]

namespace DevelopmentWithADot.AspNetSignalRVideoStreaming
{
	public class VideoStreaming : WebControl
	{
		public const String Url = "/videostreaming";

		public VideoStreaming() : base("video")
		{
			this.Interval = 100;
			this.OnStreamed = String.Empty;
			this.ScalingMode = VideoScalingMode.None;
			this.Source = false;
			this.StreamingMode = VideoStreamingMode.Event;
			this.TargetClientID = String.Empty;
			this.Resolution = VideoResolution.Default;
			this.HubUrl = Url;
		}

		[DefaultValue(Url)]
		public String HubUrl
		{
			get;
			set;
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

		[DefaultValue(VideoScalingMode.None)]
		public VideoScalingMode ScalingMode
		{
			get;
			set;
		}

		[DefaultValue(VideoResolution.Default)]
		public VideoResolution Resolution
		{
			get;
			set;
		}

		public static void Configuration(IAppBuilder app)
		{
			app.MapSignalR(Url, new HubConfiguration());
		}

		protected String GetResolutionContraint()
		{
			switch (this.Resolution)
			{
				case VideoResolution.High:
					return ("video: { mandatory: { minWidth: 1024, minHeight: 768 } }");

				case VideoResolution.Low:
					return ("video: { mandatory: { minWidth: 640, minHeight: 480 } }");

				case VideoResolution.Medium:
					return ("video: { mandatory: { minWidth: 800, minHeight: 600 } }");
			}

			return ("video: true");
		}

		protected override void OnLoad(EventArgs e)
		{
			var sm = ScriptManager.GetCurrent(this.Page);
			var streamingAction = String.Empty;
			var size = (this.ScalingMode == VideoScalingMode.OriginalSize) ? ", imageWidth, imageHeight" : (this.ScalingMode == VideoScalingMode.TargetSize) ? ", canvas.width, canvas.height" : (this.ScalingMode == VideoScalingMode.ControlSize) ? String.Format(", {0}, {1}", this.Width.Value, this.Height.Value) : String.Empty;

			switch (this.StreamingMode)
			{
				case VideoStreamingMode.Event:
					if (String.IsNullOrWhiteSpace(this.OnStreamed) == true)
					{
						throw (new InvalidOperationException("OnStreamed cannot be empty when using streaming mode Event"));
					}
					streamingAction = String.Format("{0}(imageUrl, imageWidth, imageHeight)", this.OnStreamed);
					break;

				case VideoStreamingMode.Target:
					if (String.IsNullOrWhiteSpace(this.TargetClientID) == true)
					{
						throw (new InvalidOperationException("TargetClientID cannot be empty when using streaming mode Target"));
					}
					streamingAction = String.Format("var canvas = document.getElementById('{0}'); if (canvas.tagName == 'CANVAS') {{ var ctx = canvas.getContext('2d'); var img = new Image(); }} else if (canvas.tagName == 'IMG') {{ var img = canvas; }}; img.src = imageUrl; img.width = imageWidth; img.height = imageHeight; if (typeof(ctx) != 'undefined') {{ img.onload = function() {{\n ctx.drawImage(img, 0, 0{1}); \n}} }};", this.TargetClientID, size);
					break;

				case VideoStreamingMode.Window:
					streamingAction = String.Format("if (typeof(window.videoWindow) == 'undefined') {{ window.videoWindow = window.open(imageUrl, '_blank', 'width=imageWidth,height=imageHeight'); }} else {{ window.videoWindow.location.href = imageUrl; }};");
					break;
			}

			var initScript = String.Format("\ndocument.getElementById('{0}').connection = $.hubConnection('{1}', {{ useDefaultPath: false }}); document.getElementById('{0}').proxy = document.getElementById('{0}').connection.createHubProxy('videoStreamingHub');\n", this.ClientID, this.HubUrl);
			var startStreamScript = String.Format("\ndocument.getElementById('{0}').startStream = function(){{\n var video = document.getElementById('{0}'); video.proxy.on('send', function(imageUrl, imageWidth, imageHeight) {{\n {2} \n}}); video.connection.start().done(function() {{\n if ((true == {3}) && (video.paused == true) && (video.src == '')) {{\n navigator.getUserMedia = (navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia); navigator.getUserMedia({{ {4}, audio: false }}, function (stream) {{\n video.src = window.URL.createObjectURL(stream); \n}}, function (error) {{\n debugger; \n}}); \n}}; if (video.intervalId) {{\n window.cancelAnimationFrame(video.intervalId); \n}}; var fn = function(time) {{\nif (time >= {1} && video.intervalId != 0) {{ var canvas = document.createElement('canvas'); var context = canvas.getContext('2d'); context.drawImage(video, 0, 0, canvas.width, canvas.height); var picture = canvas.toDataURL(); video.proxy.invoke('send', picture, video.videoWidth, video.videoHeight); }}; window.requestAnimationFrame(fn); \n}}; if (true == {3}) {{ video.intervalId = window.requestAnimationFrame(fn); }}; video.play(); \n}}) }}\n", this.ClientID, this.Interval, streamingAction, this.Source.ToString().ToLower(), this.GetResolutionContraint());
			var stopStreamScript = String.Format("\ndocument.getElementById('{0}').stopStream = function(){{ var video = document.getElementById('{0}'); if (video.intervalId) {{ window.cancelAnimationFrame(video.intervalId); }}; video.intervalId = 0; video.pause(); video.connection.stop(); }};\n", this.ClientID);
			var script = String.Concat(initScript, startStreamScript, stopStreamScript);

			if (sm != null)
			{
				this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat(this.HubUrl, this.ClientID), String.Format("Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function() {{ {0} }});\n", script), true);
			}
			else
			{
				this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat(this.HubUrl, this.ClientID), script, true);
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
