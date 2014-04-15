using System;
using System.Web.UI;

namespace DevelopmentWithADot.AspNetSignalRVideoStre.Test
{
	public partial class Default : Page
	{
		protected override void OnInit(EventArgs e)
		{
			var source = false;

			Boolean.TryParse(this.Request.QueryString["Source"], out source);

			this.video.Source = source;

			base.OnInit(e);
		}
	}
}