namespace RdcMan {
	internal class PluginContext : IPluginContext {
		IMainForm IPluginContext.MainForm => Program.TheForm;

		IServerTree IPluginContext.Tree => ServerTree.Instance;
	}
}
