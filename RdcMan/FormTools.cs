using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	public static class FormTools
	{
		public const int TabControlWidth = 520;

		public const int TabControlHeight = 350;

		public const int ControlHeight = 20;

		public const int VerticalSpace = 4;

		public const int HorizontalSpace = 22;

		public const int HorizontalMargin = 8;

		public const int TopMargin = 16;

		public const int BottomMargin = 16;

		public const int Indent = 24;

		public const int IndexIndent = 148;

		public const int TabPageWidth = 512;

		public const int TabPageHeight = 334;

		public const int TabPageInternalVerticalSpace = 8;

		public const int TabPageControlWidth = 480;

		public const int TabPageControlHeight = 302;

		public const int LabelWidth = 140;

		public const int TextBoxWidth = 340;

		public const int DropDownWidth = 340;

		public const int GroupBoxWidth = 496;

		public static int XPos(int colIndex)
		{
			return HorizontalMargin + IndexIndent * colIndex;
		}

		public static int YPos(int rowIndex)
		{
			return TopMargin + Indent * rowIndex;
		}

		public static int YPosNoMargin(int rowIndex)
		{
			return Indent * rowIndex;
		}

		public static Point TopLeftLocation()
		{
			return new Point(VerticalSpace, HorizontalSpace);
		}

		public static Point NewLocation(int colIndex, int rowIndex)
		{
			return new Point(XPos(colIndex), YPos(rowIndex));
		}

		public static Point NewUngroupedLocation(int colIndex, int rowIndex)
		{
			return new Point(TopMargin + LabelWidth * colIndex, YPos(rowIndex));
		}

		public static Label NewLabel(string text, int colIndex, int rowIndex)
		{
			Label label = new Label {
				Location = NewLocation(colIndex, rowIndex),
				Text = text,
				TextAlign = ContentAlignment.MiddleLeft,
				Size = new Size(LabelWidth, ControlHeight)
			};
			return label;
		}

		public static RdcTextBox NewTextBox(int colIndex, int rowIndex, int tabIndex)
		{
			RdcTextBox rdcTextBox = new RdcTextBox {
				Enabled = false,
				Location = NewLocation(colIndex, rowIndex),
				Size = new Size(TextBoxWidth, ControlHeight),
				TabIndex = tabIndex
			};
			return rdcTextBox;
		}

		public static RdcTextBox NewTextBox(int colIndex, int rowIndex, int tabIndex, int height)
		{
			RdcTextBox rdcTextBox = new RdcTextBox {
				Location = NewLocation(colIndex, rowIndex),
				Size = new Size(TextBoxWidth, ControlHeight * height),
				TabIndex = tabIndex,
				Multiline = true,
				AcceptsReturn = true,
				ScrollBars = ScrollBars.Vertical
			};
			return rdcTextBox;
		}

		public static RdcTextBox AddLabeledTextBox(Control parent, string text, ref int rowIndex, ref int tabIndex)
		{
			Label value = NewLabel(text, 0, rowIndex);
			RdcTextBox rdcTextBox = NewTextBox(1, rowIndex++, tabIndex++);
			parent.Controls.Add(value);
			parent.Controls.Add(rdcTextBox);
			return rdcTextBox;
		}

		public static RdcTextBox AddLabeledTextBox(Control parent, string text, StringSetting setting, ref int rowIndex, ref int tabIndex)
		{
			RdcTextBox rdcTextBox = AddLabeledTextBox(parent, text, ref rowIndex, ref tabIndex);
			rdcTextBox.Setting = setting;
			return rdcTextBox;
		}

		public static RdcCheckBox NewCheckBox(string text, int colIndex, int rowIndex, int tabIndex)
		{
			return NewCheckBox(text, colIndex, rowIndex, tabIndex, TextBoxWidth);
		}

		public static RdcCheckBox NewCheckBox(string text, int colIndex, int rowIndex, int tabIndex, int width)
		{
			RdcCheckBox rdcCheckBox = new() {
				Location = NewLocation(colIndex, rowIndex),
				Size = new Size(width, ControlHeight),
				TabIndex = tabIndex,
				Text = text
			};
			return rdcCheckBox;
		}

		public static RdcCheckBox AddCheckBox(Control parent, string text, BoolSetting setting, int colIndex, ref int rowIndex, ref int tabIndex)
		{
			RdcCheckBox rdcCheckBox = new RdcCheckBox
			{
				Setting = setting,
				Location = NewLocation(colIndex, rowIndex++),
				Size = new Size(TextBoxWidth, ControlHeight),
				TabIndex = tabIndex++,
				Text = text
			};
			parent.Controls.Add(rdcCheckBox);
			return rdcCheckBox;
		}

		public static ValueComboBox<TEnum> AddLabeledEnumDropDown<TEnum>(Control parent, string text, ref int rowIndex, ref int tabIndex, Func<TEnum, string> toString) where TEnum : struct
		{
			return AddLabeledValueDropDown(parent, text, ref rowIndex, ref tabIndex, toString, Helpers.EnumValues<TEnum>());
		}

		public static ValueComboBox<TValue> AddLabeledValueDropDown<TValue>(Control parent, string text, ref int rowIndex, ref int tabIndex, Func<TValue, string> toString, IEnumerable<TValue> values)
		{
			Label value = NewLabel(text, 0, rowIndex);
			ValueComboBox<TValue> valueComboBox = new ValueComboBox<TValue>(values, toString);
			SetDropDownProperties(valueComboBox, 1, rowIndex++, tabIndex++);
			parent.Controls.Add(value);
			parent.Controls.Add(valueComboBox);
			return valueComboBox;
		}

		public static ValueComboBox<TValue> AddLabeledValueDropDown<TValue>(Control parent, string text, Setting<TValue> setting, ref int rowIndex, ref int tabIndex, Func<TValue, string> toString, IEnumerable<TValue> values)
		{
			Label value = NewLabel(text, 0, rowIndex);
			ValueComboBox<TValue> valueComboBox = new ValueComboBox<TValue>(setting, values, toString);
			SetDropDownProperties(valueComboBox, 1, rowIndex++, tabIndex++);
			parent.Controls.Add(value);
			parent.Controls.Add(valueComboBox);
			return valueComboBox;
		}

		public static ValueComboBox<TEnum> AddLabeledEnumDropDown<TEnum>(Control parent, string text, EnumSetting<TEnum> setting, ref int rowIndex, ref int tabIndex, Func<TEnum, string> toString) where TEnum : struct
		{
			Label value = NewLabel(text, 0, rowIndex);
			ValueComboBox<TEnum> valueComboBox = new ValueComboBox<TEnum>(setting, Helpers.EnumValues<TEnum>(), toString);
			SetDropDownProperties(valueComboBox, 1, rowIndex++, tabIndex++);
			parent.Controls.Add(value);
			parent.Controls.Add(valueComboBox);
			return valueComboBox;
		}

		private static void SetDropDownProperties(ComboBox comboBox, int colIndex, int rowIndex, int tabIndex)
		{
			comboBox.Location = NewLocation(colIndex, rowIndex);
			comboBox.Size = new Size(DropDownWidth, ControlHeight);
			comboBox.TabIndex = tabIndex;
		}

		public static void LayoutGroupBox(GroupBox groupBox, int numCols, Control previousGroupBox)
		{
			LayoutGroupBox(groupBox, numCols, previousGroupBox, 0, 0);
		}

		public static void LayoutGroupBox(GroupBox groupBox, int numCols, Control previousControl, int rowIndex, int tabIndex)
		{
			int num = 0;
			foreach (Control control in groupBox.Controls)
			{
				if (num == 1 && control.Width == TextBoxWidth)
				{
					control.Width -= HorizontalMargin;
				}
				control.Location = NewLocation(num++, rowIndex);
				control.TabIndex = tabIndex;
				tabIndex += 2;
				if (!(control is Label))
				{
					control.TabStop = true;
				}
				if (num == numCols)
				{
					num = 0;
					rowIndex++;
				}
			}
			groupBox.SizeAndLocate(previousControl);
		}

		public static void AddControlsAndSizeGroup(this GroupBox groupBox, params Control[] controls)
		{
			groupBox.Controls.AddRange(controls);
			foreach (Control control in groupBox.Controls)
			{
				if (control.Width == TextBoxWidth)
				{
					control.Width -= HorizontalMargin;
				}
			}
		}

		public static void SizeAndLocate(this GroupBox groupBox, Control previousControl)
		{
			int num = 8;
			if (previousControl != null)
			{
				groupBox.TabIndex = previousControl.TabIndex + 1;
				num += previousControl.Bottom;
			}
			else
			{
				groupBox.TabIndex = 1;
			}
			groupBox.Location = new Point(HorizontalMargin, num);
			ResizeGroupBox(groupBox);
		}

		public static void ResizeGroupBox(GroupBox groupBox)
		{
			int num = ComputeControlHeightFromChildren(groupBox);
			groupBox.Size = new Size(GroupBoxWidth, num + HorizontalMargin);
		}

		public static TabPage NewTabPage(string name)
		{
			TabPage tabPage = new TabPage
			{
				Location = TopLeftLocation(),
				Size = new Size(TabPageWidth, TabPageHeight),
				Text = name
			};
			tabPage.SuspendLayout();
			return tabPage;
		}

		public static TabPage NewTabPage(this TabControl parent, string name)
		{
			TabPage tabPage = NewTabPage(name);
			parent.Controls.Add(tabPage);
			return tabPage;
		}

		public static Control[] NewSizeRadios()
		{
			Control[] array = new Control[SizeHelper.StockSizes.Length];
			int num = 0;
			Size[] stockSizes = SizeHelper.StockSizes;
			foreach (Size size in stockSizes)
			{
				array[num++] = new RadioButton
				{
					Tag = size,
					Text = size.ToFormattedString()
				};
			}
			return array;
		}

		public static void AddButtonsAndSizeForm(Form form, Button okButton, Button cancelButton)
		{
			int num = 0;
			int num2 = 0;
			foreach (Control control in form.Controls)
			{
				num = Math.Max(num, control.Right);
				num2 = Math.Max(num2, control.Bottom);
			}
			num += HorizontalMargin;
			cancelButton.Location = new Point(num - cancelButton.Width - HorizontalMargin - 1, num2 + TopMargin);
			okButton.Location = new Point(cancelButton.Location.X - okButton.Width - HorizontalMargin, cancelButton.Location.Y);
			form.AcceptButton = okButton;
			form.CancelButton = cancelButton;
			form.Controls.Add(cancelButton);
			form.Controls.Add(okButton);
			form.ClientSize = new Size(num, okButton.Location.Y + okButton.Height + TopMargin);
		}

		public static int ComputeControlHeightFromChildren(Control control)
		{
			int num = 0;
			foreach (Control control2 in control.Controls)
			{
				num = Math.Max(num, control2.Bottom);
			}
			return num;
		}

		public static void ErrorDialog(string text)
		{
			MessageBox.Show(Program.TheForm, text, "RDCMan Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}

		public static void InformationDialog(string text)
		{
			MessageBox.Show(Program.TheForm, text, "RDCMan", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}

		public static DialogResult ExclamationDialog(string text, MessageBoxButtons buttons)
		{
			return MessageBox.Show(Program.TheForm, text, "RDCMan", buttons, MessageBoxIcon.Exclamation);
		}

		public static DialogResult YesNoDialog(string text)
		{
			return YesNoDialog(Program.TheForm, text, MessageBoxDefaultButton.Button2);
		}

		public static DialogResult YesNoDialog(string text, MessageBoxDefaultButton defaultButton)
		{
			return YesNoDialog(Program.TheForm, text, defaultButton);
		}

		public static DialogResult YesNoDialog(Form owner, string text)
		{
			return YesNoDialog(owner, text, MessageBoxDefaultButton.Button2);
		}

		public static DialogResult YesNoDialog(Form owner, string text, MessageBoxDefaultButton defaultButton)
		{
			return MessageBox.Show(owner, text, "RDCMan", MessageBoxButtons.YesNo, MessageBoxIcon.Question, defaultButton);
		}

		public static DialogResult YesNoCancelDialog(string text)
		{
			return YesNoCancelDialog(text, MessageBoxDefaultButton.Button2);
		}

		public static DialogResult YesNoCancelDialog(string text, MessageBoxDefaultButton defaultButton)
		{
			return MessageBox.Show(Program.TheForm, text, "RDCMan", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, defaultButton);
		}

		public static void ScaleAndLayout(this Form form)
		{
			form.PerformAutoScale();
			form.ResumeLayout(performLayout: false);
			form.PerformLayout();
		}
	}
}
