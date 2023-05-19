using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using RdcMan.Configuration;

namespace RdcMan
{
	internal class Program
	{
		private delegate void ShowFormDelegate(Form form);

		private class PluginConfig
		{
			public IPlugin Plugin { get; set; }

			public string Name { get; set; }

			public XmlNode SettingsNode { get; set; }
		}

		internal static CredentialsStore CredentialsProfiles = new CredentialsStore();

		private static ApplicationContext _appContext;

		public static bool ResetPreferences = false;

		private static bool _openFiles = true;

		private static ReconnectServerOptions _reconnectServersAtStart = ReconnectServerOptions.Ask;

		private static readonly List<string> _filesToOpen = new List<string>();

		private static string[] _serversToConnect = null;

		private static readonly List<IBuiltInVirtualGroup> _builtInVirtualGroups = new List<IBuiltInVirtualGroup>();

		private const string PluginPattern = "Plugin.*.dll";

		internal static MainForm TheForm { get; set; }

		internal static Preferences Preferences { get; private set; }

		internal static ManualResetEvent InitializedEvent { get; private set; }

		internal static IEnumerable<IBuiltInVirtualGroup> BuiltInVirtualGroups => _builtInVirtualGroups;

		private static Dictionary<string, PluginConfig> Plugins { get; set; }

		private static PluginContext PluginContext { get; set; }

		public static void PluginAction(Action<IPlugin> action)
		{
			Plugins.Values.ForEach(delegate(PluginConfig v)
			{
				IPlugin plugin = v.Plugin;
				if (plugin != null)
				{
					action(plugin);
				}
			});
		}

		[STAThread]
		internal static void Main(params string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: true);
			Policies.Read();
			using (Helpers.Timer("parsing command line"))
			{
				ParseCommandLine();
			}
			try
			{
				Current.Read();
			}
			catch (Exception ex)
			{
				FormTools.ErrorDialog("读取 RDCMan 配置文件时出错：{0} 程序可能不稳定和/或功能不完整。".InvariantFormat(ex.Message));
			}
			using (CompositionContainer compositionContainer = new CompositionContainer(new AssemblyCatalog(Assembly.GetCallingAssembly())))
			{
				_builtInVirtualGroups.AddRange(compositionContainer.GetExportedValues<IBuiltInVirtualGroup>());
				_builtInVirtualGroups.Sort((IBuiltInVirtualGroup a, IBuiltInVirtualGroup b) => a.Text.CompareTo(b.Text));
			}
			using (Helpers.Timer("reading preferences"))
			{
				Preferences = Preferences.Load();
				if (Preferences == null)
				{
					Environment.Exit(1);
				}
			}
			Thread thread;
			using (Helpers.Timer("starting message loop thread"))
			{
				InitializedEvent = new ManualResetEvent(initialState: false);
				thread = new Thread(StartMessageLoop)
				{
					IsBackground = true
				};
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				InitializedEvent.WaitOne();
			}
			if (TheForm == null)
			{
				Environment.Exit(1);
			}
			TheForm.Invoke(new MethodInvoker(CompleteInitialization));
			thread.Join();
			Log.Write("Exiting");
		}

		private static void CompleteInitialization()
		{
			InstantiatePlugins();
			if (_filesToOpen.Count > 0)
			{
				Preferences.FilesToOpen = _filesToOpen;
			}
			else if (!_openFiles)
			{
				Preferences.FilesToOpen = null;
			}
			List<ServerBase> connectedServers = new List<ServerBase>();
			ServerTree.Instance.Operation(OperationBehavior.SuspendSort, delegate
			{
				foreach (IBuiltInVirtualGroup item in BuiltInVirtualGroups.Where((IBuiltInVirtualGroup group) => group.IsVisibilityConfigurable))
				{
					item.IsInTree = Preferences.GetBuiltInGroupVisibility(item);
				}
			});
			OpenFiles();
			ServerTree.Instance.Operation(OperationBehavior.SuspendGroupChanged, delegate
			{
				Preferences.LoadBuiltInGroups();
				ConnectedGroup.Instance.Nodes.ForEach(delegate(TreeNode n)
				{
					connectedServers.Add(((ServerRef)n).ServerNode);
				});
				ConnectedGroup.Instance.RemoveChildren();
				ServerTree.Instance.SortBuiltinGroups();
			});
			ServerTree.Instance.Show();
			ServerTree.Instance.Focus();
			bool isFirstConnection = ReconnectAtStartup(connectedServers);
			if (_serversToConnect != null)
			{
				ConnectNamedServers(_serversToConnect, isFirstConnection);
			}
			if (Preferences.ServerTreeVisibility != 0)
			{
				ServerTree.Instance.Hide();
			}
			PluginAction(delegate(IPlugin p)
			{
				p.PostLoad(PluginContext);
			});
			Preferences.NeedToSave = false;
			TheForm.UpdateAutoSaveTimer();
			ThreadPool.QueueUserWorkItem(delegate
			{
				CheckForUpdate();
			});
			Log.Write("Startup completed");
		}

		private static bool ReconnectAtStartup(List<ServerBase> connectedServers)
		{
			IEnumerable<ServerBase> reconnectServers = Enumerable.Empty<ServerBase>();
			switch (_reconnectServersAtStart)
			{
			case ReconnectServerOptions.All:
				reconnectServers = connectedServers;
				break;
			case ReconnectServerOptions.None:
				return false;
			case ReconnectServerOptions.Ask:
				if (Preferences.ReconnectOnStartup && connectedServers.Any())
				{
					reconnectServers = new List<ServerBase>(ConnectServersDialog(connectedServers));
				}
				break;
			}
			return ConnectServers(reconnectServers, isFirstConnection: true);
		}

		private static void InstantiatePlugins()
		{
			PluginContext = new PluginContext();
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			DirectoryCatalog catalog = new DirectoryCatalog(Path.GetDirectoryName(callingAssembly.Location), "Plugin.*.dll");
			CompositionContainer compositionContainer = new CompositionContainer(catalog);
			Plugins = new Dictionary<string, PluginConfig>(StringComparer.OrdinalIgnoreCase);
			StringBuilder stringBuilder = new StringBuilder();
			XmlNode value = Preferences.Settings.PluginSettings.Value;
			if (value != null)
			{
				foreach (XmlNode item in value.SelectNodes("//plugin"))
				{
					try
					{
						string value2 = item.Attributes["path"].Value;
						if (!string.IsNullOrEmpty(value2))
						{
							PluginConfig value3 = new PluginConfig
							{
								Name = value2,
								SettingsNode = item
							};
							Plugins[value2] = value3;
						}
					}
					catch
					{
					}
				}
			}
			foreach (IPlugin item2 in from e in compositionContainer.GetExports<IPlugin>()
				select e.Value)
			{
				string name = item2.GetType().Assembly.GetName().Name;
				if (!Plugins.TryGetValue(name, out var value4))
				{
					value4 = new PluginConfig
					{
						Name = name
					};
					Plugins[name] = value4;
				}
				try
				{
					item2.PreLoad(PluginContext, value4.SettingsNode);
					value4.Plugin = item2;
				}
				catch (Exception ex)
				{
					stringBuilder.AppendLine("加载“{0}”时出错：{1}".InvariantFormat(name, ex.Message));
				}
			}
			foreach (PluginConfig item3 in Plugins.Values.Where((PluginConfig c) => c.Plugin == null))
			{
				stringBuilder.AppendLine("“{0}”以前使用过，但现在未加载".InvariantFormat(item3.Name));
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine().Append("Click Cancel to exit");
				string text = "一些插件尚未加载。 RDCMan 可能无法按预期运行。{0}{0}".InvariantFormat(Environment.NewLine) + stringBuilder.ToString();
				if (FormTools.ExclamationDialog(text, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
				{
					Environment.Exit(1);
				}
			}
		}

		private static void OpenFiles()
		{
			List<string> filesToOpen = Preferences.FilesToOpen;
			if (filesToOpen == null)
			{
				return;
			}
			bool flag = true;
			foreach (string item in filesToOpen)
			{
				FileGroup fileGroup = RdgFile.OpenFile(item);
				if (fileGroup != null && flag)
				{
					flag = false;
					ServerTree.Instance.SelectedNode = fileGroup;
				}
			}
		}

		internal static IEnumerable<ServerBase> ConnectServersDialog(IEnumerable<ServerBase> servers)
		{
			using ConnectServersDialog connectServersDialog = new ConnectServersDialog(servers);
			if (connectServersDialog.ShowDialog(TheForm) == DialogResult.OK)
			{
				return connectServersDialog.SelectedServers.ToList();
			}
			return new ServerBase[0];
		}

		internal static bool ConnectServers(IEnumerable<ServerBase> reconnectServers, bool isFirstConnection)
		{
			NodeHelper.ThrottledConnect(reconnectServers, delegate(ServerBase server)
			{
				if (isFirstConnection)
				{
					ServerTree.Instance.SelectedNode = server;
					server.Focus();
					isFirstConnection = false;
				}
			});
			return isFirstConnection;
		}

		private static bool ConnectNamedServers(ICollection<string> serverNames, bool isFirstConnection)
		{
			HashSet<string> nameHash = new HashSet<string>(serverNames, StringComparer.OrdinalIgnoreCase);
			List<ServerBase> serversToConnect = new List<ServerBase>();
			ServerTree.Instance.Nodes.VisitNodes(delegate(RdcTreeNode node)
			{
				if (node is Server server && nameHash.Contains(server.ServerName))
				{
					if (!server.IsConnected)
					{
						serversToConnect.Add(server);
					}
					nameHash.Remove(server.ServerName);
				}
			});
			isFirstConnection = ConnectServers(serversToConnect, isFirstConnection);
			if (nameHash.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder("未找到以下服务器，无法连接：").AppendLine().AppendLine();
				foreach (string item in nameHash)
				{
					stringBuilder.AppendLine(item);
				}
				FormTools.InformationDialog(stringBuilder.ToString());
			}
			return isFirstConnection;
		}

		private static void ParseCommandLine()
		{
			ArgumentParser argumentParser = new ArgumentParser();
			argumentParser.AddSwitch("?", requiresValue: false);
			argumentParser.AddSwitch("h", requiresValue: false);
			argumentParser.AddSwitch("reset", requiresValue: false);
			argumentParser.AddSwitch("noopen", requiresValue: false);
			argumentParser.AddSwitch("noconnect", requiresValue: false);
			argumentParser.AddSwitch("reconnect", requiresValue: false);
			argumentParser.AddSwitch("c", requiresValue: true);
			try
			{
				argumentParser.Parse();
			}
			catch (ArgumentException ex)
			{
				FormTools.ErrorDialog(ex.Message);
				Environment.Exit(1);
			}
			if (argumentParser.HasSwitch("?") || argumentParser.HasSwitch("h"))
			{
				Usage();
				Environment.Exit(0);
			}
			if (argumentParser.HasSwitch("reset"))
			{
				ResetPreferences = true;
			}
			if (argumentParser.HasSwitch("noopen"))
			{
				_openFiles = false;
			}
			if (argumentParser.HasSwitch("noconnect"))
			{
				_reconnectServersAtStart = ReconnectServerOptions.None;
			}
			if (argumentParser.HasSwitch("reconnect"))
			{
				_reconnectServersAtStart = ReconnectServerOptions.All;
			}
			if (argumentParser.HasSwitch("c"))
			{
				_serversToConnect = argumentParser.SwitchValues["c"].Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			}
			_filesToOpen.AddRange(argumentParser.PlainArgs);
		}

		internal static void Usage() {
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string text = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), "Resources\\help.htm");
			if (File.Exists(text)) {
				text = text.Replace('\\', '/');
				Process.Start(text);
			}
			else
				FormTools.InformationDialog("未能找到帮助文件:\r\n" + text);
		}

		private static void StartMessageLoop()
		{
			try
			{
				if (MainForm.Create() != null)
				{
					Log.Init();
					_appContext = new ApplicationContext(TheForm);
					Application.Run(_appContext);
				}
			}
			finally
			{
				InitializedEvent.Set();
			}
		}

		internal static void ShowForm(Form form)
		{
			_appContext.MainForm.Invoke(new ShowFormDelegate(ShowFormWorker), form);
		}

		private static void ShowFormWorker(Form form)
		{
			form.Show();
			form.BringToFront();
		}

		private static void CheckForUpdate()
		{
			ProgramUpdateElement updateElement = Current.RdcManSection?.ProgramUpdate;
			try
			{
				if (string.IsNullOrEmpty(updateElement?.VersionPath) || string.IsNullOrEmpty(updateElement.UpdateUrl))
				{
					return;
				}
				if (!DateTime.TryParse(Preferences.LastUpdateCheckTimeUtc, out var result) || DateTime.UtcNow.Subtract(result).TotalDays < 1.0)
				{
					Log.Write("Last checked for update on {0}, not checking until tomorrow", result.ToString("s"));
					return;
				}
				Preferences.LastUpdateCheckTimeUtc = DateTime.UtcNow.ToString("u");
				string input = File.ReadAllText(updateElement.VersionPath);
				if (!Version.TryParse(input, out var result2))
				{
					return;
				}
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				AssemblyName name = executingAssembly.GetName();
				Log.Write("Latest version = {0}", result2);
				if (name.Version < result2)
				{
					TheForm.Invoke((MethodInvoker)delegate
					{
						FormTools.InformationDialog("T{0} 提供了新版本的 RDCMan".InvariantFormat(updateElement.UpdateUrl));
					});
				}
			}
			catch (Exception ex)
			{
				FormTools.InformationDialog("检测 RDCMan 新版本出错 {0}\r\n{1}".InvariantFormat(updateElement.UpdateUrl,ex.Message));
			}
		}
	}
}
