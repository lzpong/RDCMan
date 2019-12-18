using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	public abstract class RdcDialog : Form
	{
		protected Button _acceptButton;

		protected Button _cancelButton;

		private readonly Dictionary<Control, ErrorProvider> _errorProviders;

		protected RdcDialog(string dialogTitle, string acceptButtonText)
		{
			_errorProviders = new Dictionary<Control, ErrorProvider>();
			SuspendLayout();
			base.AutoScaleDimensions = new SizeF(96f, 96f);
			base.AutoScaleMode = AutoScaleMode.Dpi;
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			Text = dialogTitle;
			_acceptButton = new Button
			{
				Text = acceptButtonText
			};
			_cancelButton = new Button();
			_cancelButton.Click += CancelButton_Click;
			base.Shown += ShownCallback;
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			_errorProviders.ForEach(delegate(KeyValuePair<Control, ErrorProvider> kvp)
			{
				kvp.Value.Clear();
			});
		}

		protected RdcDialog(string dialogTitle, string acceptButtonText, Form parentForm)
			: this(dialogTitle, acceptButtonText)
		{
			if (parentForm != null)
			{
				base.StartPosition = FormStartPosition.Manual;
				base.Location = new Point(parentForm.Location.X + 10, parentForm.Location.Y + 20);
			}
		}

		public bool SetError(Control c, string text)
		{
			if (!_errorProviders.TryGetValue(c, out ErrorProvider value))
			{
				value = new ErrorProvider();
				value.SetIconAlignment(c, ErrorIconAlignment.MiddleLeft);
				_errorProviders[c] = value;
			}
			value.SetError(c, text);
			return !string.IsNullOrEmpty(text);
		}

		public virtual void InitButtons()
		{
			_cancelButton.TabIndex = 100;
			_cancelButton.Text = "Cancel";
			_cancelButton.DialogResult = DialogResult.Cancel;
			_acceptButton.TabIndex = 99;
			_acceptButton.Click += AcceptIfValid;
			FormTools.AddButtonsAndSizeForm(this, _acceptButton, _cancelButton);
		}

		protected virtual void ShownCallback(object sender, EventArgs args)
		{
		}

		protected void Close(DialogResult dr)
		{
			base.DialogResult = dr;
			Close();
		}

		protected void OK()
		{
			Close(DialogResult.OK);
		}

		protected void Cancel()
		{
			Close(DialogResult.Cancel);
		}

		protected virtual void AcceptIfValid(object sender, EventArgs e)
		{
			if (ValidateControls(base.Controls.FlattenControls(), isValid: true))
			{
				OK();
			}
		}

		public bool ValidateControls(IEnumerable<Control> controls, bool isValid)
		{
			foreach (Control control in controls)
			{
				ISettingControl settingControl = control as ISettingControl;
				if (settingControl != null && control.Enabled)
				{
					string text = settingControl.Validate();
					if (SetError(control, text) && isValid)
					{
						control.Focus();
						isValid = false;
					}
				}
			}
			return isValid;
		}
	}
}
