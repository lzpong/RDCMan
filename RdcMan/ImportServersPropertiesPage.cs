using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RdcMan
{
	internal class ImportServersPropertiesPage : NodePropertiesPage<SettingsGroup>
	{
		private RdcTextBox _fileNameTextBox;

		private RdcTextBox _serversTextBox;

		public List<string> ExpandedServerNames
		{
			get;
			private set;
		}

		public ImportServersPropertiesPage(TabbedSettingsDialog dialog)
			: base(dialog, (SettingsGroup)null, "Server Settings")
		{
			int tabIndex = 0;
			int num = 0;
			Label value = new Label
			{
				Location = FormTools.NewLocation(0, num),
				Size = new Size(480, 48),
				Text = "选择包含服务器信息的文件，或在下面的文本框中输入信息。 服务器名称由逗号和换行符分隔。 允许扩展。"
			};
			num += 2;
			base.Controls.Add(value);
			Button browseButton = new Button
			{
				TabIndex = tabIndex++,
				Text = "&B浏览"
			};
			browseButton.Click += OnBrowseClick;
			_fileNameTextBox = FormTools.AddLabeledTextBox(this, "&File name:", ref num, ref tabIndex);
			_fileNameTextBox.Enabled = true;
			_fileNameTextBox.Width -= browseButton.Width + 8;
			browseButton.Location = new Point(_fileNameTextBox.Right + 8, _fileNameTextBox.Top);
			_serversTextBox = FormTools.NewTextBox(1, num, tabIndex++, 7);
			_serversTextBox.AcceptsReturn = true;
			_serversTextBox.Enabled = true;
			_serversTextBox.ScrollBars = ScrollBars.Vertical;
			num += 6;
			base.Controls.Add(browseButton, _serversTextBox);
			AddParentCombo(ref num, ref tabIndex);
			RdcTextBox fileNameTextBox = _fileNameTextBox;
			EventHandler value2 = delegate
			{
				_serversTextBox.Enabled = string.IsNullOrEmpty(_fileNameTextBox.Text);
			};
			fileNameTextBox.TextChanged += value2;
			_serversTextBox.TextChanged += delegate
			{
				RdcTextBox fileNameTextBox2 = _fileNameTextBox;
				bool enabled = browseButton.Enabled = string.IsNullOrEmpty(_serversTextBox.Text);
				fileNameTextBox2.Enabled = enabled;
			};
			base.FocusControl = _fileNameTextBox;
		}

		protected override bool IsValid()
		{
			Control c = _serversTextBox;
			string text = _serversTextBox.Text;
			base.Dialog.SetError(_serversTextBox, null);
			base.Dialog.SetError(_fileNameTextBox, null);
			if (!string.IsNullOrEmpty(_fileNameTextBox.Text))
			{
				c = _fileNameTextBox;
				try
				{
					text = File.ReadAllText(_fileNameTextBox.Text);
				}
				catch (Exception ex)
				{
					base.Dialog.SetError(_fileNameTextBox, ex.Message);
					return false;
				}
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				base.Dialog.SetError(_fileNameTextBox, "请输入文件名");
				return false;
			}
			try
			{
				List<string> list = new List<string>();
				text = text.Replace(Environment.NewLine, ",");
				MatchCollection matchCollection = Regex.Matches(text, "([^,\\{\\s]*\\{[^\\}]*\\}[^,\\{,\\}\\s]*)|([^,\\{\\}\\s]+)");
				foreach (Match item in matchCollection)
				{
					list.AddRange(StringUtilities.ExpandPattern(item.Groups[0].Value.Trim()));
				}
				ExpandedServerNames = list;
			}
			catch (Exception ex2)
			{
				base.Dialog.SetError(c, ex2.Message);
			}
			return true;
		}

		protected override bool CanBeParent(GroupBase group)
		{
			return group.CanAddServers();
		}

		private void OnBrowseClick(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Title = "导入";
				openFileDialog.DefaultExt = "txt";
				openFileDialog.AddExtension = true;
				openFileDialog.CheckFileExists = true;
				openFileDialog.RestoreDirectory = false;
				openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					_fileNameTextBox.Text = openFileDialog.FileName;
				}
			}
		}
	}
}
