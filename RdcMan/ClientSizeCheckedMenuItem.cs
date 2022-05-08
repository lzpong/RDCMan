using System.Drawing;

namespace RdcMan {
	internal class ClientSizeCheckedMenuItem : CheckedMenuItem {
		private readonly RdcBaseForm _form;

		public ClientSizeCheckedMenuItem(RdcBaseForm form, Size size)
			: base(size.ToFormattedString()) {
			base.Tag = size;
			_form = form;
		}

		protected override void CheckChanged(bool isChecked) {
			Size clientSize = (Size)base.Tag;
			_form.SetClientSize(clientSize);
		}

		public override void Update() {
			Size size = (Size)base.Tag;
			Size clientSize = _form.GetClientSize();
			base.Checked = clientSize == size;
		}
	}
}
