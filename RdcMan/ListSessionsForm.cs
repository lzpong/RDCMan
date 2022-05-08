using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Win32;

namespace RdcMan {
	internal class ListSessionsForm : Form {
		//private IContainer components;

		public Label StatusLabel;

		public ListView SessionListView;

		private ColumnHeader idHeader1;

		private ColumnHeader stateHeader1;

		private ColumnHeader userHeader1;

		private ColumnHeader clientHeader1;

		private Button RefreshButton;

		private readonly RemoteSessions _remoteSessions;

		private int[] _sortOrder = new int[1];

		private readonly object _queryLock = new object();

		private bool _areQuerying;

		protected override void Dispose(bool disposing) {
			//if (disposing && components != null)
			//	components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent() {
			base.SuspendLayout();
			this.StatusLabel = new Label {
				Location = new Point(12, 9),
				Name = "StatusLabel",
				Size = new System.Drawing.Size(238, 24),
				TabIndex = 0,
				Text = "正在查询会话..."
			};
			this.idHeader1 = new ColumnHeader {
				Text = "Id",
				Width = 30
			};
			this.stateHeader1 = new ColumnHeader {
				Text = "State",
				Width = 80
			};
			this.userHeader1 = new ColumnHeader {
				Text = "User",
				Width = 135
			};
			this.clientHeader1 = new ColumnHeader {
				Text = "Client",
				Width = 96
			};
			this.SessionListView = new ListView {
				FullRowSelect = true,
				Location = new System.Drawing.Point(10, 42),
				MultiSelect = false,
				Name = "SessionListView",
				Size = new System.Drawing.Size(345, 154),
				TabIndex = 1,
				UseCompatibleStateImageBehavior = false,
				View = View.Details
			};
			this.SessionListView.ColumnClick += new ColumnClickEventHandler(SessionListView_ColumnClick);
			this.SessionListView.Columns.AddRange(new ColumnHeader[4] { this.idHeader1, this.stateHeader1, this.userHeader1, this.clientHeader1 });
			this.RefreshButton = new Button {
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(256, 9),
				Name = "RefreshButton",
				Size = new Size(100, 24),
				TabIndex = 2,
				Text = "刷新",
				UseVisualStyleBackColor = true
			};
			this.RefreshButton.Click += new EventHandler(RefreshButton_Click);
			base.Controls.Add(this.RefreshButton);
			base.Controls.Add(this.SessionListView);
			base.Controls.Add(this.StatusLabel);
			base.ClientSize = new Size(366, 206);
			base.Name = "ListSessionsForm";
			base.Load += new EventHandler(ListSessionsForm_Load);
			base.SizeChanged += new EventHandler(ListSessionsForm_SizeChanged);
			base.FormClosed += new FormClosedEventHandler(ListSessionsForm_FormClosed);
			base.Resize += new EventHandler(ListSessionsForm_Resize);
			base.ResumeLayout(false);
		}

		public ListSessionsForm(ServerBase server) {
			InitializeComponent();
			_remoteSessions = new RemoteSessions(server);
			_areQuerying = true;
			Text = server.ServerName + " 会话";
			ContextMenu contextMenu = new ContextMenu();
			contextMenu.Popup += OnContextMenu;
			SessionListView.ContextMenu = contextMenu;
			base.StartPosition = FormStartPosition.Manual;
			base.Location = Program.TheForm.Location;
			base.Location.Offset(100, 100);
			base.Icon = Program.TheForm.Icon;
		}

		private ListViewItem GetSelection() {
			ListView sessionListView = SessionListView;
			if (sessionListView.SelectedItems.Count != 1)
				return null;

			IEnumerator enumerator = sessionListView.SelectedItems.GetEnumerator();
			enumerator.MoveNext();
			return enumerator.Current as ListViewItem;
		}

		private void ListSessionsForm_Load(object sender, EventArgs e) {
			ThreadPool.QueueUserWorkItem(OpenThreadProc, this);
		}

		private static void OpenThreadProc(object o) {
			ListSessionsForm form = o as ListSessionsForm;
			if (!form._remoteSessions.OpenServer()) {
				form.Invoke((MethodInvoker)delegate {
					form.StatusLabel.Text = "无法访问远程会话";
				});
			}
			else
				QuerySessions(form);
		}

		private static void QuerySessions(object o) {
			ListSessionsForm form = o as ListSessionsForm;
			if (form.IsDisposed) {
				return;
			}
			form.Invoke((MethodInvoker)delegate {
				form.SessionListView.BeginUpdate();
				form.SessionListView.Items.Clear();
			});
			IList<RemoteSessionInfo> list = form._remoteSessions.QuerySessions();
			if (list == null) {
				form.Invoke((MethodInvoker)delegate {
					form.StatusLabel.Text = "无法枚举远程会话";
				});
				return;
			}
			foreach (RemoteSessionInfo item in list) {
				Wts.ConnectstateClass connectstateClass = item.State;
				string text = ((item.DomainName.Length <= 0) ? item.UserName : (item.DomainName + "\\" + item.UserName));
				ListViewItem value = new ListViewItem {
					Text = item.SessionId.ToString()
				};
				value.SubItems.Add(connectstateClass.ToString());
				value.SubItems.Add(text);
				value.SubItems.Add(item.ClientName);
				form.Invoke((MethodInvoker)delegate {
					form.SessionListView.Items.Add(value);
				});
			}
			form.Invoke((MethodInvoker)delegate {
				int count = form.SessionListView.Items.Count;
				form.StatusLabel.Text = count + " 个会话";
				form.SortListView();
				form.SessionListView.EndUpdate();
				form._areQuerying = false;
			});
		}

		private void ListSessionsForm_FormClosed(object sender, FormClosedEventArgs e) {
			_remoteSessions.CloseServer();
		}

		private void RefreshButton_Click(object sender, EventArgs e) {
			RefreshSessions();
		}

		private void RefreshSessions() {
			lock (_queryLock) {
				if (_areQuerying)
					return;

				_areQuerying = true;
			}
			StatusLabel.Text = "刷新中...";
			ThreadPool.QueueUserWorkItem(QuerySessions, this);
		}

		private void OnContextMenu(object sender, EventArgs e) {
			ContextMenu contextMenu = sender as ContextMenu;
			ListViewItem selection = GetSelection();
			if (selection != null) {
				contextMenu.MenuItems.Clear();
				Wts.ConnectstateClass connectstateClass;
				switch (selection.SubItems[1].Text) {
					case "Active":
					case "Connected":
						connectstateClass = Wts.ConnectstateClass.Connected;
						break;
					case "Disconnected":
						connectstateClass = Wts.ConnectstateClass.Disconnected;
						break;
					default:
						connectstateClass = Wts.ConnectstateClass.ConnectQuery;
						break;
				}
				MenuItem item = new MenuItem("断开(&D)", DisconnectSession) {
					Enabled = (connectstateClass == Wts.ConnectstateClass.Connected)
				};
				contextMenu.MenuItems.Add(item);
				contextMenu.MenuItems.Add("-");
				item = new MenuItem("注销(&L)", LogOffSession);
				contextMenu.MenuItems.Add(item);
				item.Enabled = !Policies.DisableLogOff;
			}
		}

		private void DisconnectSession(object sender, EventArgs e) {
			ListViewItem selection = GetSelection();
			if (int.TryParse(selection.SubItems[0].Text, out var result)) {
				_remoteSessions.DisconnectSession(result);
				RefreshSessions();
			}
		}

		private void LogOffSession(object sender, EventArgs e) {
			ListViewItem selection = GetSelection();
			if (int.TryParse(selection.SubItems[0].Text, out var result)) {
				_remoteSessions.LogOffSession(result);
				RefreshSessions();
			}
		}

		private void ListSessionsForm_Resize(object sender, EventArgs e) {
			SessionListView.Width = RefreshButton.Right - SessionListView.Left;
		}

		private void SessionListView_ColumnClick(object sender, ColumnClickEventArgs e) {
			_sortOrder = new int[1];
			int num = 0;
			_sortOrder[num++] = e.Column;
			SortListView();
		}

		private void SortListView() {
			ArrayList arrayList = new ArrayList(SessionListView.Items.Count);
			foreach (ListViewItem item in SessionListView.Items)
				arrayList.Add(item);

			arrayList.Sort(new SessionListSortComparer(_sortOrder));
			SessionListView.Items.Clear();

			foreach (ListViewItem item2 in arrayList)
				SessionListView.Items.Add(item2);
		}

		private void ListSessionsForm_SizeChanged(object sender, EventArgs e) {
			int num = Math.Max(20, base.ClientRectangle.Width - SessionListView.Location.X * 2);
			int num2 = Math.Max(20, base.ClientRectangle.Height - SessionListView.Location.Y - 10);
			SessionListView.Size = new Size(num, num2);
		}
	}
}
