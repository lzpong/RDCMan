namespace RdcMan
{
	public class ServerDisplaySettingsTabPage : DisplaySettingsTabPage<ServerDisplaySettings>
	{
		public ServerDisplaySettingsTabPage(TabbedSettingsDialog dialog, ServerDisplaySettings settings)
			: base(dialog, settings)
		{
			Create(out var _, out var _);
		}
	}
}
