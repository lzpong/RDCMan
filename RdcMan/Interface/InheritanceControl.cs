using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan
{
	public class InheritanceControl
	{
		private const string SourcePrefix = "Source: ";

		public CheckBox FromParentCheck;

		private Button _sourceButton;

		private Label _disabledLabel;

		protected readonly TabbedSettingsDialog _dialog;

		private RdcTreeNode _sourceNode;

		private readonly string _settingsGroupName;

		private bool _enabled;

		public event Action<bool> EnabledChanged;

		public InheritanceControl(TabbedSettingsDialog dialog, string settingsGroupName)
		{
			_dialog = dialog;
			_settingsGroupName = settingsGroupName;
			_sourceNode = DefaultSettingsGroup.Instance;
			_enabled = true;
		}

		public void Create(Control parent, ref int rowIndex, ref int tabIndex)
		{
			_disabledLabel = new Label
			{
				Enabled = true,
				Location = new Point(0, (parent.Height - 20) / 2),
				Size = new Size(parent.Width, 20),
				TextAlign = ContentAlignment.MiddleCenter,
				Visible = false
			};
			FromParentCheck = FormTools.NewCheckBox("In&herit from parent", 1, rowIndex++, tabIndex++);
			FromParentCheck.CheckedChanged += CheckChangedHandler;
			_sourceButton = new Button
			{
				Location = FormTools.NewLocation(1, rowIndex++)
			};
			_sourceButton.Size = new Size(340, _sourceButton.Height);
			_sourceButton.Click += SourceButton_Click;
			_sourceButton.TextChanged += SourceButton_TextChanged;
			parent.Controls.Add(_disabledLabel);
			parent.Controls.Add(FromParentCheck);
			parent.Controls.Add(_sourceButton);
		}

		private void SourceButton_Click(object sender, EventArgs e)
		{
			TabPage tabPage = _sourceButton.Parent as TabPage;
			string activeTabName = (tabPage != null) ? tabPage.Text : string.Empty;
			_sourceNode.DoPropertiesDialog(_sourceButton.FindForm(), activeTabName);
		}

		private void SourceButton_TextChanged(object sender, EventArgs e)
		{
			string text = _sourceButton.Text;
			Graphics graphics = _sourceButton.CreateGraphics();
			bool flag = false;
			SizeF sizeF = graphics.MeasureString(text, _sourceButton.Font);
			while (Math.Round(sizeF.Width, 1) > (double)_sourceButton.Width)
			{
				double num = Math.Round(sizeF.Width, 0) - (double)_sourceButton.Width;
				int num2 = (int)Math.Round(num / (double)_sourceButton.Font.Size, 0) + 4;
				text = "Source: ..." + text.Substring(num2 + "Source: ".Length);
				flag = true;
				sizeF = graphics.MeasureString(text, _sourceButton.Font);
			}
			if (flag)
			{
				_sourceButton.Text = text;
			}
		}

		public void UpdateControlsFromSettings(InheritSettingsType settings)
		{
			bool flag = settings.Mode == InheritanceMode.FromParent;
			if (flag != FromParentCheck.Checked)
			{
				FromParentCheck.Checked = flag;
			}
			else
			{
				OnSettingChanged();
			}
		}

		public void Enable(bool value, string reason)
		{
			_enabled = value;
			_disabledLabel.Text = "These settings are unavailable {0}".InvariantFormat(reason);
			foreach (Control control in FromParentCheck.Parent.Controls)
			{
				control.Visible = _enabled;
			}
			_disabledLabel.Enabled = !_enabled;
			_disabledLabel.Visible = !_enabled;
			if (_enabled)
			{
				OnSettingChanged();
			}
		}

		private void CheckChangedHandler(object sender, EventArgs e)
		{
			OnSettingChanged();
		}

		private void OnSettingChanged()
		{
			CheckBox fromParentCheck = FromParentCheck;
			EnableDisableControls(!fromParentCheck.Checked);
			if (fromParentCheck.Checked)
			{
				GroupBase groupBase = _dialog.TabPages.OfType<INodePropertiesPage>().First().ParentGroup;
				if (groupBase != _sourceNode)
				{
					if (groupBase == null)
					{
						_sourceNode = DefaultSettingsGroup.Instance;
					}
					else
					{
						while (true)
						{
							SettingsGroup settingsGroupByName = groupBase.GetSettingsGroupByName(_settingsGroupName);
							if (settingsGroupByName.InheritSettingsType.Mode != 0)
							{
								break;
							}
							groupBase = settingsGroupByName.InheritSettingsType.GetInheritedSettingsNode(groupBase);
						}
						_sourceNode = groupBase;
					}
				}
				if (_sourceNode != DefaultSettingsGroup.Instance)
				{
					_sourceButton.Text = "Source: " + _sourceNode.FullPath;
				}
				else
				{
					_sourceButton.Text = "Source: Default settings group";
				}
				_sourceButton.Show();
			}
			else
			{
				_sourceButton.Hide();
			}
		}

		public void EnableDisableControls(bool enable)
		{
			foreach (Control control in FromParentCheck.Parent.Controls)
			{
				if (control != FromParentCheck && control != _sourceButton)
				{
					control.Enabled = enable;
				}
			}
			if (this.EnabledChanged != null)
			{
				this.EnabledChanged(enable);
			}
		}

		public InheritanceMode GetInheritanceMode()
		{
			if (FromParentCheck.Checked)
			{
				return InheritanceMode.FromParent;
			}
			return InheritanceMode.None;
		}
	}
}
