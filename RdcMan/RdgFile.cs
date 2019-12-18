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

		private const string SaveFileFilter = "RDCMan Groups (*.rdg)|*.rdg";

		private const string OpenFileFilter = "RDCMan Groups (*.rdg)|*.rdg";

		private static int _saveInProgress;

		private static string CurrentWorkingDirectory;

		public static FileGroup NewFile()
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Title = "New File";
				saveFileDialog.Filter = "RDCMan Groups (*.rdg)|*.rdg";
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
		}

		public static void CloseFileGroup(FileGroup file)
		{
			file.AnyOrAllConnected(out bool anyConnected, out bool _);
			if (anyConnected)
			{
				DialogResult dialogResult = FormTools.YesNoDialog("There are active connections from " + file.Text + ". Are you sure you want to close it?");
				if (dialogResult == DialogResult.No)
				{
					return;
				}
			}
			if (SaveFileGroup(file) != 0)
			{
				ServerTree.Instance.RemoveNode(file);
				Program.Preferences.NeedToSave = true;
			}
		}

		public static FileGroup OpenFile()
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Title = "Open";
				openFileDialog.DefaultExt = "rdg";
				openFileDialog.AddExtension = true;
				openFileDialog.CheckFileExists = true;
				openFileDialog.InitialDirectory = GetWorkingDirectory();
				openFileDialog.RestoreDirectory = false;
				openFileDialog.Filter = "RDCMan Groups (*.rdg)|*.rdg";
				if (openFileDialog.ShowDialog() != DialogResult.OK)
				{
					return null;
				}
				CurrentWorkingDirectory = Path.GetDirectoryName(openFileDialog.FileName);
				return OpenFile(openFileDialog.FileName);
			}
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
					xmlTextReader = new XmlTextReader(filename);
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
					throw new FileLoadException(filename + ": File format error");
				}
				FileGroup fileGroup = new FileGroup(filename);
				FileGroup fileGroup2 = (from f in ServerTree.Instance.Nodes.OfType<FileGroup>()
					where f.Pathname.Equals(fileGroup.Pathname, StringComparison.OrdinalIgnoreCase)
					select f).FirstOrDefault();
				if (fileGroup2 == null)
				{
					try
					{
						List<string> errors = new List<string>();
						ServerTree.Instance.Operation((OperationBehavior)31, delegate
						{
							ServerTree.Instance.AddNode(fileGroup, ServerTree.Instance.RootNode);
							if (!ReadXml(topNode, fileGroup, errors))
							{
								throw new Exception(string.Empty);
							}
						});
						if (errors.Count > 0)
						{
							StringBuilder stringBuilder = new StringBuilder("The following errors were encountered:").AppendLine().AppendLine();
							foreach (string item in errors)
							{
								stringBuilder.AppendLine(item);
							}
							stringBuilder.AppendLine().Append("The file was not loaded completely. If it is saved it almost certainly means losing information. Continue?");
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
							GroupBase groupBase = node as GroupBase;
							if (groupBase != null && groupBase.Properties.Expanded.Value)
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
				FormTools.InformationDialog("{0} is already open as '{1}'".CultureFormat(fileGroup.Pathname, fileGroup2.Text));
				return fileGroup2;
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
			if (num > 3)
			{
				DialogResult dialogResult = FormTools.YesNoDialog("{0} was written by a newer version of RDCMan ({1}). It may not load properly. If it does and is saved by this version, it will revert to the older file schema possibly losing information. Continue?".CultureFormat(fileGroup.GetFilename(), text));
				if (dialogResult == DialogResult.No)
				{
					return false;
				}
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
				Helpers.ReadXmlDelegate readXmlDelegate2 = nodeActions[virtualGroup.XmlNodeName] = delegate(XmlNode childNode, RdcTreeNode group, ICollection<string> errors2)
				{
					virtualGroup.ReadXml(childNode, fileGroup, errors2);
				};
			}
			nodeActions["version"] = delegate
			{
			};
			LongRunningActionForm.PerformOperation("Opening " + fileGroup.Pathname, showImmediately: false, delegate
			{
				foreach (XmlNode childNode in topNode.ChildNodes)
				{
					if (nodeActions.TryGetValue(childNode.Name, out Helpers.ReadXmlDelegate value))
					{
						value(childNode, fileGroup, errors);
					}
					else
					{
						errors.Add("Unexpected Xml node {0} in '{1}'".CultureFormat(childNode.GetFullPath(), fileGroup.GetFilename()));
					}
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
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Title = "Save";
				saveFileDialog.Filter = "RDCMan Groups (*.rdg)|*.rdg";
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
				xmlTextWriter.WriteAttributeString("schemaVersion", 3.ToString());
				fileGroup.WriteXml(xmlTextWriter);
				foreach (IBuiltInVirtualGroup item in Program.BuiltInVirtualGroups.Where((IBuiltInVirtualGroup v) => !string.IsNullOrEmpty(v.XmlNodeName)))
				{
					item.WriteXml(xmlTextWriter, fileGroup);
				}
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteEndDocument();
				xmlTextWriter.Close();
				xmlTextWriter = null;
				Helpers.MoveTemporaryToPermanent(temporaryFileName, fileGroup.Pathname, fileGroup.SchemaVersion != 3);
				fileGroup.SchemaVersion = 3;
				fileGroup.HasChangedSinceWrite = false;
				return SaveResult.Save;
			}
			catch (Exception ex)
			{
				xmlTextWriter?.Close();
				switch (FormTools.YesNoCancelDialog(ex.Message + "\n\nTry again? (Selecting Cancel will preserve the original file)"))
				{
				case DialogResult.Cancel:
					return SaveResult.Cancel;
				case DialogResult.Yes:
					return SaveResult.Retry;
				default:
					return SaveResult.NoSave;
				}
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
			{
				return SaveResult.NoSave;
			}
			if (Program.Preferences.AutoSaveFiles)
			{
				return SaveResult.AutoSave;
			}
			if (!file.HasChangedSinceWrite)
			{
				return SaveResult.AutoSave;
			}
			string text = "Save changes to " + file.GetFilename() + "?";
			switch (FormTools.YesNoCancelDialog(text, MessageBoxDefaultButton.Button1))
			{
			case DialogResult.Cancel:
				return SaveResult.Cancel;
			case DialogResult.No:
				return SaveResult.NoSave;
			default:
				return SaveResult.Save;
			}
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
