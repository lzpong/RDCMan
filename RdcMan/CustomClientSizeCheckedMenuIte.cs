using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan {
	internal class CustomClientSizeCheckedMenuItem : CheckedMenuItem {
		private RdcBaseForm _form;

		private string _baseText;

		public CustomClientSizeCheckedMenuItem(RdcBaseForm form, string text)
			: base(text) {
			_baseText = Text;
			_form = form;
		}

		protected override void CheckChanged(bool isChecked) {
			Size clientSize = _form.GetClientSize();
			using CustomSizeDialog customSizeDialog = new CustomSizeDialog(clientSize);
			if (customSizeDialog.ShowDialog() == DialogResult.OK) {
				Size clientSize2 = SizeHelper.FromString(customSizeDialog.WidthText, customSizeDialog.HeightText);
				_form.SetClientSize(clientSize2);
			}
		}

		public override void Update() {
			Size clientSize = _form.GetClientSize();
			bool flag2 = (base.Checked = SizeHelper.StockSizes.All((Size size) => size != clientSize));
			string text = _baseText;
			if (flag2)
				text += " ({0})".InvariantFormat(clientSize.ToFormattedString());

			Text = text + "...";
		}
	}
}
