using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RdcMan
{
	internal class SmartGroupPropertiesTabPage : GroupBasePropertiesTabPage<SmartGroupSettings>
	{
		private class SmartRuleControl : Control
		{
			private readonly ValueComboBox<ServerProperty> _propertyCombo;

			private readonly ValueComboBox<RuleOperator> _operatorCombo;

			private readonly TextBox _valueTextBox;

			public ServerProperty Property => _propertyCombo.SelectedValue;

			public RuleOperator Operator => _operatorCombo.SelectedValue;

			public Control ValueControl => _valueTextBox;

			public object Value => _valueTextBox.Text;

			public Button AddButton
			{
				get;
				private set;
			}

			public Button DeleteButton
			{
				get;
				private set;
			}

			public int Index
			{
				get;
				set;
			}

			public SmartRuleControl(Rule rule, ref int tabIndex)
			{
				_propertyCombo = new ValueComboBox<ServerProperty>(Helpers.EnumValues<ServerProperty>(), (ServerProperty v) => v.ToString())
				{
					Location = new Point(0, 0),
					Width = 100,
					TabIndex = tabIndex++,
					SelectedValue = ServerProperty.DisplayName
				};
				_operatorCombo = new ValueComboBox<RuleOperator>(Helpers.EnumValues<RuleOperator>(), (RuleOperator v) => v.ToString())
				{
					Location = new Point(_propertyCombo.Right + 4, 0),
					Width = 100,
					TabIndex = tabIndex++,
					SelectedValue = RuleOperator.Matches
				};
				_valueTextBox = new TextBox
				{
					Enabled = true,
					Location = new Point(_operatorCombo.Right + 4, 0),
					Width = 459 - (_operatorCombo.Right + 4) - 48,
					TabIndex = tabIndex++
				};
				DeleteButton = new Button
				{
					Enabled = true,
					Location = new Point(_valueTextBox.Right + 4, 0),
					Size = new Size(20, 20),
					TabIndex = tabIndex++,
					Text = "-"
				};
				AddButton = new Button
				{
					Enabled = true,
					Location = new Point(DeleteButton.Right + 4, 0),
					Size = new Size(20, 20),
					TabIndex = tabIndex++,
					Text = "+"
				};
				base.Controls.Add(_propertyCombo, _operatorCombo, _valueTextBox, DeleteButton, AddButton);
				base.Size = new Size(459, 21);
				base.TabStop = false;
				if (rule != null)
				{
					_propertyCombo.SelectedValue = rule.Property.ServerProperty;
					_operatorCombo.SelectedValue = rule.Operator;
					_valueTextBox.Text = rule.Value.ToString();
				}
			}
		}

		private const int RuleWidth = 459;

		private const int RuleHeight = 21;

		private readonly RuleGroup _ruleGroup;

		private readonly Panel _rulePanel;

		private readonly ValueComboBox<RuleGroupOperator> _anyAllCombo;

		private int _nextRuleTabIndex;

		public SmartGroupPropertiesTabPage(TabbedSettingsDialog dialog, SmartGroupSettings settings)
			: base(dialog, settings, settings.Name)
		{
			_ruleGroup = ((dialog as SmartGroupPropertiesDialog).AssociatedNode as SmartGroup).RuleGroup;
			int rowIndex = 0;
			int tabIndex = 0;
			AddGroupName(ref rowIndex, ref tabIndex);
			AddParentCombo(ref rowIndex, ref tabIndex);
			rowIndex++;
			Label label = new Label
			{
				Location = FormTools.NewLocation(0, rowIndex++),
				Text = "Servers which match",
				TextAlign = ContentAlignment.MiddleLeft,
				Size = new Size(110, 20)
			};
			_anyAllCombo = new ValueComboBox<RuleGroupOperator>(Helpers.EnumValues<RuleGroupOperator>(), (RuleGroupOperator v) => v.ToString())
			{
				Location = new Point(label.Right, label.Top),
				Size = new Size(50, 20),
				TabIndex = tabIndex++,
				SelectedValue = RuleGroupOperator.All
			};
			Label label2 = new Label
			{
				Location = new Point(_anyAllCombo.Right + 5, label.Top),
				Text = "of the following rules",
				TextAlign = ContentAlignment.MiddleLeft,
				Size = new Size(140, 20)
			};
			base.Controls.Add(label, _anyAllCombo, label2);
			GroupBox groupBox = new GroupBox
			{
				Location = FormTools.NewLocation(0, rowIndex++)
			};
			_rulePanel = new Panel
			{
				Location = FormTools.NewLocation(0, 0),
				AutoScroll = true
			};
			int num = 302 - groupBox.Top - 40;
			_rulePanel.Size = new Size(480, num);
			_rulePanel.VerticalScroll.LargeChange = num;
			_rulePanel.VerticalScroll.SmallChange = num / 20;
			groupBox.Size = new Size(496, num + _rulePanel.Top * 2);
			groupBox.Controls.Add(_rulePanel);
			_nextRuleTabIndex = tabIndex;
			base.Controls.Add(groupBox);
		}

		protected override void UpdateControls()
		{
			base.UpdateControls();
			_anyAllCombo.SelectedValue = _ruleGroup.Operator;
			if (_ruleGroup.Rules.Count > 0)
			{
				_ruleGroup.Rules.ForEach(AddRuleControl);
			}
			else
			{
				AddRuleControl(null);
			}
			LayoutRuleControls();
		}

		protected override bool IsValid()
		{
			bool flag = true;
			foreach (SmartRuleControl control in _rulePanel.Controls)
			{
				string text = null;
				try
				{
					string text2 = control.Value as string;
					if (text2 != null)
					{
						if (string.IsNullOrEmpty(text2))
						{
							text = "Please enter a pattern";
						}
						else
						{
							Regex.Match(string.Empty, text2);
						}
					}
				}
				catch (Exception ex)
				{
					text = ex.Message;
				}
				flag &= !base.Dialog.SetError(control.ValueControl, text);
			}
			if (flag)
			{
				return base.IsValid();
			}
			return false;
		}

		protected override void UpdateSettings()
		{
			base.UpdateSettings();
			List<SmartRuleControl> source = _rulePanel.Controls.Cast<SmartRuleControl>().ToList();
			IEnumerable<Rule> rules = from r in source
				orderby r.Index
				select r into c
				select new Rule(new RuleProperty(c.Property), c.Operator, c.Value);
			_ruleGroup.Set(_anyAllCombo.SelectedValue, rules);
		}

		protected override void ParentGroupChangedHandler(object sender, EventArgs e)
		{
		}

		private void InsertRuleControl(SmartRuleControl afterRule)
		{
			int num = afterRule.Index + 1;
			foreach (SmartRuleControl control in _rulePanel.Controls)
			{
				if (control.Index >= num)
				{
					control.Index++;
				}
			}
			SmartRuleControl value = CreateRuleControl(null, num);
			_rulePanel.Controls.Add(value);
			LayoutRuleControls();
		}

		private void AddRuleControl(Rule rule)
		{
			SmartRuleControl value = CreateRuleControl(rule, _rulePanel.Controls.Count);
			_rulePanel.Controls.Add(value);
		}

		private SmartRuleControl CreateRuleControl(Rule rule, int index)
		{
			SmartRuleControl newRule = new SmartRuleControl(rule, ref _nextRuleTabIndex)
			{
				Index = index
			};
			newRule.AddButton.Click += delegate
			{
				InsertRuleControl(newRule);
			};
			newRule.DeleteButton.Click += delegate
			{
				DeleteRuleControl(newRule);
			};
			return newRule;
		}

		private void DeleteRuleControl(SmartRuleControl rule)
		{
			int index = rule.Index;
			_rulePanel.Controls.Remove(rule);
			foreach (SmartRuleControl control in _rulePanel.Controls)
			{
				if (control.Index > index)
				{
					control.Index--;
				}
			}
			LayoutRuleControls();
		}

		private void LayoutRuleControls()
		{
			int count = _rulePanel.Controls.Count;
			int num = 0;
			_rulePanel.SuspendLayout();
			int value = _rulePanel.VerticalScroll.Value;
			foreach (SmartRuleControl control in _rulePanel.Controls)
			{
				control.DeleteButton.Enabled = (count > 1);
				control.Location = new Point(0, control.Index * 25 - value);
				num = Math.Max(num, control.Top);
			}
			_rulePanel.VerticalScroll.Maximum = num;
			_rulePanel.VerticalScroll.Value = Math.Min(num, value);
			_rulePanel.ResumeLayout();
		}
	}
}
