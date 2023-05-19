using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	internal static class RdgFile
	{
		private const int CurrentSchemaVersion = 3;

		private const string RDCManFileFilter = "RDCMan Groups (*.rdg)|*.rdg";

		//private const string OpenFileFilter = "RDCMan Groups (*.rdg)|*.rdg";

		private static int _saveInProgress;

		private static string CurrentWorkingDirectory;

		public static FileGroup NewFile()
		{
			using SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Title = "新建文件";
			saveFileDialog.Filter = RDCManFileFilter;
			saveFileDialog.AddExtension = true;
			saveFileDialog.CheckPathExists = true;
			saveFileDialog.InitialDirectory = GetWorkingDirectory();
			saveFileDialog.RestoreDirectory = false;
			if (saveFileDialog.ShowDialog() != DialogResult.OK)
			{
				return null;
			}
			FileGroup fileGroup = new FileGroup(saveFileDialog.FileName);
			ServerTree.Instance.AddNode(fileGroup, ServerTree.Instance.RootNode);
			DoSaveWithRetry(fileGroup);
			return fileGroup;
		}

		public static void CloseFileGroup(FileGroup file)
		{
			file.AnyOrAllConnected(out var anyConnected, out var _);
			if (anyConnected) {
				DialogResult dialogResult = FormTools.YesNoDialog("有来自 " + file.Text + " 的活动连接。确定要关闭它吗？");
				if (dialogResult == DialogResult.No)
					return;
			}
			if (SaveFileGroup(file) != 0) {
				ServerTree.Instance.RemoveNode(file);
				Program.Preferences.NeedToSave = true;
			}
		}

		public static FileGroup OpenFile()
		{
			using OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "打开";
			openFileDialog.DefaultExt = "rdg";
			openFileDialog.AddExtension = true;
			openFileDialog.CheckFileExists = true;
			openFileDialog.InitialDirectory = GetWorkingDirectory();
			openFileDialog.RestoreDirectory = false;
			openFileDialog.Filter = RDCManFileFilter;
			if (openFileDialog.ShowDialog() != DialogResult.OK) {
				return null;
			}
			CurrentWorkingDirectory = Path.GetDirectoryName(openFileDialog.FileName);
			return OpenFile(openFileDialog.FileName);
		}

		public static FileGroup OpenFile(string filename)
		{
			using (Helpers.Timer("reading {0}", filename))
			{
				XmlDocument xmlDocument = new XmlDocument();
				XmlTextReader xmlTextReader = null;
				XmlNode topNode;
				try
				{
					xmlTextReader = new XmlTextReader(filename)
					{
						DtdProcessing = DtdProcessing.Ignore
					};
					xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
					xmlTextReader.MoveToContent();
					topNode = xmlDocument.ReadNode(xmlTextReader);
				}
				catch (Exception ex)
				{
					FormTools.ErrorDialog(ex.Message);
					return null;
				}
				finally
				{
					xmlTextReader?.Close();
				}
				if (topNode == null)
				{
					throw new FileLoadException(filename + ": 文件格式错误");
				}
				FileGroup fileGroup = new FileGroup(filename);
				FileGroup fileGroup2 = (from f in ServerTree.Instance.Nodes.OfType<FileGroup>()
					where f.Pathname.Equals(fileGroup.Pathname, StringComparison.OrdinalIgnoreCase)
					select f).FirstOrDefault();
				if (fileGroup2 != null)
				{
					FormTools.InformationDialog("{0} 已作为“{1}”打开".CultureFormat(fileGroup.Pathname, fileGroup2.Text));
					return fileGroup2;
				}
				try
				{
					List<string> errors = new List<string>();
					ServerTree.Instance.Operation(OperationBehavior.RestoreSelected | OperationBehavior.SuspendSort | OperationBehavior.SuspendUpdate | OperationBehavior.SuspendGroupChanged, delegate
					{
						ServerTree.Instance.AddNode(fileGroup, ServerTree.Instance.RootNode);
						if (!ReadXml(topNode, fileGroup, errors))
						{
							throw new Exception(string.Empty);
						}
					});
					if (errors.Count > 0)
					{
						StringBuilder stringBuilder = new StringBuilder("遇到以下错误：").AppendLine().AppendLine();
						foreach (string item in errors)
						{
							stringBuilder.AppendLine(item);
						}
						stringBuilder.AppendLine().Append("该文件未完全加载。如果它被保存，它几乎肯定意味着丢失信息。是否继续？");
						DialogResult dialogResult = FormTools.ExclamationDialog(stringBuilder.ToString(), MessageBoxButtons.YesNo);
						if (dialogResult == DialogResult.No)
						{
							throw new Exception(string.Empty);
						}
					}
					using (Helpers.Timer("sorting root, builtin groups and file"))
					{
						ServerTree.Instance.SortRoot();
						foreach (GroupBase builtInVirtualGroup in Program.BuiltInVirtualGroups)
						{
							ServerTree.Instance.SortGroup(builtInVirtualGroup);
							ServerTree.Instance.OnGroupChanged(builtInVirtualGroup, ChangeType.TreeChanged);
						}
						ServerTree.Instance.SortGroup(fileGroup, recurse: true);
						ServerTree.Instance.OnGroupChanged(fileGroup, ChangeType.TreeChanged);
					}
					SmartGroup.RefreshAll(fileGroup);
					fileGroup.VisitNodes(delegate(RdcTreeNode node)
					{
						if (node is GroupBase groupBase && groupBase.Properties.Expanded.Value)
						{
							groupBase.Expand();
						}
					});
					Encryption.DecryptPasswords();
					fileGroup.CheckCredentials();
					fileGroup.VisitNodes(delegate(RdcTreeNode n)
					{
						n.ResetInheritance();
					});
					fileGroup.HasChangedSinceWrite = false;
					Program.Preferences.NeedToSave = true;
					return fileGroup;
				}
				catch (Exception ex2)
				{
					if (!string.IsNullOrEmpty(ex2.Message))
					{
						FormTools.ErrorDialog(ex2.Message);
					}
					ServerTree.Instance.RemoveNode(fileGroup);
					return null;
				}
			}
		}

		private static bool ReadXml(XmlNode topNode, FileGroup fileGroup, ICollection<string> errors)
		{
			string text = "unknown";
			int num = 0;
			try
			{
				XmlNode namedItem = topNode.Attributes.GetNamedItem("programVersion");
				text = namedItem.InnerText;
			}
			catch
			{
			}
			try
			{
				XmlNode namedItem2 = topNode.Attributes.GetNamedItem("schemaVersion");
				num = int.Parse(namedItem2.InnerText);
			}
			catch
			{
			}
			fileGroup.SchemaVersion = num;
			if (num > CurrentSchemaVersion) {
				DialogResult dialogResult = FormTools.YesNoDialog("{0} 由较新版本的 RDCMan ({1}) 编写。它可能无法正确加载。如果确实如此并且被此版本保存，它将恢复到旧的文件模式，可能会丢失信息。是否继续？".CultureFormat(fileGroup.GetFilename(), text));
				if (dialogResult == DialogResult.No)
					return false;
			}
			GroupBase.SchemaVersion = num;
			if (GroupBase.SchemaVersion <= 2)
			{
				fileGroup.EncryptionSettings.InheritSettingsType.Mode = InheritanceMode.None;
			}
			Dictionary<string, Helpers.ReadXmlDelegate> nodeActions = new Dictionary<string, Helpers.ReadXmlDelegate>();
			nodeActions["file"] = delegate(XmlNode childNode, RdcTreeNode group, ICollection<string> errors2)
			{
				(group as GroupBase).ReadXml(childNode, errors2);
			};
			foreach (IBuiltInVirtualGroup virtualGroup in Program.BuiltInVirtualGroups.Where((IBuiltInVirtualGroup v) => !string.IsNullOrEmpty(v.XmlNodeName)))
			{
				nodeActions[virtualGroup.XmlNodeName] = delegate(XmlNode childNode, RdcTreeNode group, ICollection<string> errors2)
				{
					virtualGroup.ReadXml(childNode, fileGroup, errors2);
				};
			}
			nodeActions["version"] = delegate
			{
			};
			LongRunningActionForm.PerformOperation("打开 " + fileGroup.Pathname, showImmediately: false, delegate
			{
				foreach (XmlNode childNode in topNode.ChildNodes)
				{
					if (nodeActions.TryGetValue(childNode.Name, out var value))
						value(childNode, fileGroup, errors);
					else
						errors.Add("“{1}”中出现意外的 Xml 节点 {0}".CultureFormat(childNode.GetFullPath(), fileGroup.GetFilename()));
				}
			});
			return true;
		}

		public static SaveResult SaveFileGroup(FileGroup file)
		{
			if (Interlocked.CompareExchange(ref _saveInProgress, 1, 0) == 1)
			{
				return SaveResult.NoSave;
			}
			try
			{
				return DoSaveWithRetry(file);
			}
			finally
			{
				_saveInProgress = 0;
			}
		}

		public static SaveResult SaveAll()
		{
			if (Interlocked.CompareExchange(ref _saveInProgress, 1, 0) == 1)
			{
				return SaveResult.NoSave;
			}
			try
			{
				return DoSaveAll(conditional: false);
			}
			finally
			{
				_saveInProgress = 0;
			}
		}

		private static SaveResult DoSaveAll(bool conditional)
		{
			foreach (FileGroup item in ServerTree.Instance.Nodes.OfType<FileGroup>())
			{
				if (!conditional || item.HasChangedSinceWrite)
				{
					SaveResult saveResult = DoSaveWithRetry(item);
					if (saveResult == SaveResult.Cancel)
					{
						return saveResult;
					}
				}
			}
			return SaveResult.Save;
		}

		public static SaveResult DoSaveWithRetry(FileGroup file)
		{
			if (!file.AllowEdit(popUI: false))
			{
				return SaveResult.NoSave;
			}
			while (true)
			{
				SaveResult saveResult = SaveFile(file);
				switch (saveResult)
				{
				case SaveResult.Retry:
					break;
				case SaveResult.Cancel:
					return saveResult;
				default:
					return SaveResult.Save;
				}
			}
		}

		public static SaveResult SaveAs(FileGroup file)
		{
			using SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Title = "保存";
			saveFileDialog.Filter = RDCManFileFilter;
			saveFileDialog.AddExtension = true;
			saveFileDialog.CheckPathExists = true;
			saveFileDialog.FileName = Path.GetFileName(file.Pathname);
			saveFileDialog.InitialDirectory = Path.GetDirectoryName(file.Pathname);
			saveFileDialog.RestoreDirectory = false;
			SaveResult saveResult;
			do
			{
				switch (saveFileDialog.ShowDialog())
				{
				case DialogResult.Cancel:
					return SaveResult.Cancel;
				default:
					return SaveResult.NoSave;
				case DialogResult.OK:
					break;
				}
				file.Pathname = Path.Combine(Directory.GetCurrentDirectory(), saveFileDialog.FileName);
				saveResult = SaveFile(file);
			}
			while (saveResult == SaveResult.Retry);
			return saveResult;
		}

		private static SaveResult SaveFile(FileGroup fileGroup)
		{
			string temporaryFileName = Helpers.GetTemporaryFileName(fileGroup.Pathname, ".new");
			XmlTextWriter xmlTextWriter = null;
			try
			{
				xmlTextWriter = new XmlTextWriter(temporaryFileName, Encoding.UTF8);
				xmlTextWriter.Formatting = Formatting.Indented;
				xmlTextWriter.Indentation = 2;
				xmlTextWriter.WriteStartDocument();
				xmlTextWriter.WriteStartElement("RDCMan");
				xmlTextWriter.WriteAttributeString("programVersion", Program.TheForm.VersionText);
				xmlTextWriter.WriteAttributeString("schemaVersion", CurrentSchemaVersion.ToString());
				fileGroup.WriteXml(xmlTextWriter);
				foreach (IBuiltInVirtualGroup item in Program.BuiltInVirtualGroups.Where((IBuiltInVirtualGroup v) => !string.IsNullOrEmpty(v.XmlNodeName)))
				{
					item.WriteXml(xmlTextWriter, fileGroup);
				}
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteEndDocument();
				xmlTextWriter.Close();
				xmlTextWriter = null;
				Helpers.MoveTemporaryToPermanent(temporaryFileName, fileGroup.Pathname, fileGroup.SchemaVersion != CurrentSchemaVersion);
				fileGroup.SchemaVersion = CurrentSchemaVersion;
				fileGroup.HasChangedSinceWrite = false;
				return SaveResult.Save;
			}
			catch (Exception ex)
			{
				xmlTextWriter?.Close();
				return FormTools.YesNoCancelDialog(ex.Message + "\n\n再试一次？（选择取消将保留原始文件）") switch {
					DialogResult.Cancel => SaveResult.Cancel,
					DialogResult.Yes => SaveResult.Retry,
					_ => SaveResult.NoSave,
				};
			}
		}

		public static bool AutoSave()
		{
			if (Interlocked.CompareExchange(ref _saveInProgress, 1, 0) == 1)
			{
				return false;
			}
			try
			{
				DoSaveAll(conditional: true);
			}
			finally
			{
				_saveInProgress = 0;
			}
			return true;
		}

		public static SaveResult ShouldSaveFile(FileGroup file)
		{
			if (!file.AllowEdit(popUI: false))
				return SaveResult.NoSave;
			if (Program.Preferences.AutoSaveFiles)
				return SaveResult.AutoSave;
			if (!file.HasChangedSinceWrite)
				return SaveResult.AutoSave;
			string text = "将更改保存到 " + file.GetFilename() + " ？";
			return FormTools.YesNoCancelDialog(text, MessageBoxDefaultButton.Button1) switch {
				DialogResult.Cancel => SaveResult.Cancel,
				DialogResult.No => SaveResult.NoSave,
				_ => SaveResult.Save,
			};
		}

		private static string GetWorkingDirectory()
		{
			FileGroup selectedFile = ServerTree.Instance.GetSelectedFile();
			if (selectedFile != null)
			{
				return selectedFile.GetDirectory();
			}
			return CurrentWorkingDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		}
	}
}
