namespace RdcMan
{
	internal class ServerTreeVisibilityMenuItem : EnumMenuItem<ControlVisibility>
	{
		protected override ControlVisibility Value
		{
			get
			{
				return Program.TheForm.ServerTreeVisibility;
			}
			set
			{
				Program.TheForm.ServerTreeVisibility = value;
			}
		}

		public ServerTreeVisibilityMenuItem(string text, ControlVisibility value)
			: base(text, value)
		{
		}
	}
}
