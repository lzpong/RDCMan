namespace RdcMan
{
	public interface IPluginContext
	{
		IMainForm MainForm
		{
			get;
		}

		IServerTree Tree
		{
			get;
		}
	}
}
