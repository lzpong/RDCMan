using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace RdcMan {
	internal class SelectServersDialogBase : RdcDialog {
		protected const int DialogWidth = 500;

		private int _suspendItemChecked;

		public IEnumerable<ServerBase> SelectedServers {
			get {
				foreach (ListViewItem item in ListView.Items) {
					if (item.Checked)
						yield return (ServerBase)item.Tag;
				}
			}
		}

		protected RdcListView ListView { get; private set; }

		public SelectServersDialogBase(string dialogTitle, string acceptButtonText)
			: base(dialogTitle, acceptButtonText) {
		}

		protected void AddLabel(string text, ref int rowIndex, ref int tabIndex) {
			Label value = new Label {
				Location = FormTools.NewLocation(0, rowIndex++),
				Text = text,
				TextAlign = ContentAlignment.MiddleLeft,
				Size = new Size(500, 20)
			};
			base.Controls.Add(value);
		}

		protected void AddListView(ref int rowIndex, ref int tabIndex) {
			ListView = new RdcListView {
				CheckBoxes = true,
				FullRowSelect = true,
				Location = FormTools.NewLocation(0, rowIndex++),
				MultiSelect = false,
				Size = new Size(500, 300),
				TabIndex = tabIndex++,
				View = View.Details
			};
			ListView.KeyDown += List_KeyDownHandler;
			ListView.MouseDoubleClick += List_MouseDoubleClick;
			ListView.ItemChecked += ListView_ItemChecked;
			ListView.Columns.AddRange(new ColumnHeader[3]
			{
				new ColumnHeader {
					Text = string.Empty,
					Width = 22
				},
				new ColumnHeader {
					Text = "服务器",
					Width = 130
				},
				new ColumnHeader {
					Text = "组",
					Width = 349
				}
			});
			base.Controls.Add(ListView);
			if (RdcListView.SupportsHeaderCheckBoxes) {
				ListView.SetColumnHeaderToCheckBox(0);
				ListView.HeaderCheckBoxClick += ListView_HeaderCheckBoxClick;
			}
		}

		public void SuspendItemChecked() {
			Interlocked.Increment(ref _suspendItemChecked);
		}

		public void ResumeItemChecked() {
			if (Interlocked.Decrement(ref _suspendItemChecked) == 0)
				SetHeaderCheckFromItems();
		}

		protected ListViewItem CreateListViewItem(ServerBase server) {
			return new ListViewItem(new string[3] {
				"",
				server.DisplayName,
				server.Parent.FullPath
			}) {
				Tag = server
			};
		}

		public override void InitButtons() {
			base.InitButtons();
			if (!RdcListView.SupportsHeaderCheckBoxes) {
				Button button = new Button {
					Text = "全选(&A)",
					TabIndex = _acceptButton.TabIndex - 1
				};
				button.Click += SelectAll_Click;
				button.Location = new Point(8, _acceptButton.Location.Y);
				base.Controls.Add(button);
			}
		}

		private void List_MouseDoubleClick(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left)
				OK();
		}

		private void List_KeyDownHandler(object sender, KeyEventArgs e) {
			if (e.KeyData == (Keys.A | Keys.Control)) {
				e.Handled = true;
				SelectAllItems(isChecked: true);
			}
		}

		private void SelectAll_Click(object sender, EventArgs e) {
			SelectAllItems(isChecked: true);
		}

		private void SelectAllItems(bool isChecked) {
			try {
				SuspendItemChecked();
				foreach (ListViewItem item in ListView.Items) {
					item.Checked = isChecked;
				}
			}
			finally {
				ResumeItemChecked();
			}
		}

		private void ListView_ItemChecked(object sender, ItemCheckedEventArgs e) {
			if (_suspendItemChecked == 0)
				SetHeaderCheckFromItems();
		}

		private void SetHeaderCheckFromItems() {
			bool isChecked = ListView.Items.OfType<ListViewItem>().All((ListViewItem i) => i.Checked);
			ListView.SetColumnHeaderChecked(0, isChecked);
		}

		private void ListView_HeaderCheckBoxClick(object sender, HeaderColumnClickEventArgs e) {
			SelectAllItems(e.IsChecked);
		}
	}
}
