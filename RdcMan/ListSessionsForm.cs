using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Win32;

namespace RdcMan
{
	internal class ListSessionsForm : Form
	{
		private IContainer components;

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

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			StatusLabel = new System.Windows.Forms.Label();
			SessionListView = new System.Windows.Forms.ListView();
			idHeader1 = new System.Windows.Forms.ColumnHeader();
			stateHeader1 = new System.Windows.Forms.ColumnHeader();
			userHeader1 = new System.Windows.Forms.ColumnHeader();
			clientHeader1 = new System.Windows.Forms.ColumnHeader();
			RefreshButton = new System.Windows.Forms.Button();
			SuspendLayout();
			StatusLabel.Location = new System.Drawing.Point(12, 9);
			StatusLabel.Name = "StatusLabel";
			StatusLabel.Size = new System.Drawing.Size(238, 24);
			StatusLabel.TabIndex = 0;
			StatusLabel.Text = "查询会话...";
			SessionListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4]
			{
				idHeader1,
				stateHeader1,
				userHeader1,
				clientHeader1
			});
			SessionListView.FullRowSelect = true;
			SessionListView.Location = new System.Drawing.Point(10, 42);
			SessionListView.MultiSelect = false;
			SessionListView.Name = "SessionListView";
			SessionListView.Size = new System.Drawing.Size(345, 154);
			SessionListView.TabIndex = 1;
			SessionListView.UseCompatibleStateImageBehavior = false;
			SessionListView.View = System.Windows.Forms.View.Details;
			SessionListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(SessionListView_ColumnClick);
			idHeader1.Text = "Id";
			idHeader1.Width = 30;
			stateHeader1.Text = "State";
			stateHeader1.Width = 80;
			userHeader1.Text = "User";
			userHeader1.Width = 135;
			clientHeader1.Text = "Client";
			clientHeader1.Width = 96;
			RefreshButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			RefreshButton.Location = new System.Drawing.Point(256, 9);
			RefreshButton.Name = "RefreshButton";
			RefreshButton.Size = new System.Drawing.Size(100, 24);
			RefreshButton.TabIndex = 2;
			RefreshButton.Text = "刷新";
			RefreshButton.UseVisualStyleBackColor = true;
			RefreshButton.Click += new System.EventHandler(RefreshButton_Click);
			base.ClientSize = new System.Drawing.Size(366, 206);
			base.Controls.Add(RefreshButton);
			base.Controls.Add(SessionListView);
			base.Controls.Add(StatusLabel);
			base.Name = "ListSessionsForm";
			base.Load += new System.EventHandler(ListSessionsForm_Load);
			base.SizeChanged += new System.EventHandler(ListSessionsForm_SizeChanged);
			base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(ListSessionsForm_FormClosed);
			base.Resize += new System.EventHandler(ListSessionsForm_Resize);
			ResumeLayout(false);
		}

		public ListSessionsForm(ServerBase server)
		{
			InitializeComponent();
			_remoteSessions = new RemoteSessions(server);
			_areQuerying = true;
			Text = server.ServerName + " 的会话";
			ContextMenu contextMenu = new ContextMenu();
			contextMenu.Popup += OnContextMenu;
			SessionListView.ContextMenu = contextMenu;
			base.StartPosition = FormStartPosition.Manual;
			base.Location = Program.TheForm.Location;
			base.Location.Offset(100, 100);
			base.Icon = Program.TheForm.Icon;
		}

		private ListViewItem GetSelection()
		{
			ListView sessionListView = SessionListView;
			if (sessionListView.SelectedItems.Count != 1)
			{
				return null;
			}
			IEnumerator enumerator = sessionListView.SelectedItems.GetEnumerator();
			enumerator.MoveNext();
			return enumerator.Current as ListViewItem;
		}

		private void ListSessionsForm_Load(object sender, EventArgs e)
		{
			ThreadPool.QueueUserWorkItem(OpenThreadProc, this);
		}

		private static void OpenThreadProc(object o)
		{
			ListSessionsForm form = o as ListSessionsForm;
			if (!form._remoteSessions.OpenServer())
			{
				form.Invoke((MethodInvoker)delegate
				{
					form.StatusLabel.Text = "无法访问远程会话";
				});
			}
			else
			{
				QuerySessions(form);
			}
		}

		private static void QuerySessions(object o)
		{
			ListSessionsForm form = o as ListSessionsForm;
			if (!form.IsDisposed)
			{
				form.Invoke((MethodInvoker)delegate
				{
					form.SessionListView.BeginUpdate();
					form.SessionListView.Items.Clear();
				});
				IList<RemoteSessionInfo> list = form._remoteSessions.QuerySessions();
				if (list == null)
				{
					form.Invoke((MethodInvoker)delegate
					{
						form.StatusLabel.Text = "无法枚举远程会话";
					});
					return;
				}
				foreach (RemoteSessionInfo item in list)
				{
					Wts.ConnectstateClass state = item.State;
					string text = (item.DomainName.Length <= 0) ? item.UserName : (item.DomainName + '\\' + item.UserName);
					ListViewItem value = new ListViewItem
					{
						Text = item.SessionId.ToString()
					};
					value.SubItems.Add(state.ToString());
					value.SubItems.Add(text);
					value.SubItems.Add(item.ClientName);
					form.Invoke((MethodInvoker)delegate
					{
						form.SessionListView.Items.Add(value);
					});
				}
				form.Invoke((MethodInvoker)delegate
				{
					int count = form.SessionListView.Items.Count;
					string text2 = count + " 个会话";
					form.StatusLabel.Text = text2;
					form.SortListView();
					form.SessionListView.EndUpdate();
					form._areQuerying = false;
				});
			}
		}

		private void ListSessionsForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			_remoteSessions.CloseServer();
		}

		private void RefreshButton_Click(object sender, EventArgs e)
		{
			RefreshSessions();
		}

		private void RefreshSessions()
		{
			lock (_queryLock)
			{
				if (_areQuerying)
				{
					return;
				}
				_areQuerying = true;
			}
			StatusLabel.Text = "正在刷新...";
			ThreadPool.QueueUserWorkItem(QuerySessions, this);
		}

		private void OnContextMenu(object sender, EventArgs e)
		{
			ContextMenu contextMenu = sender as ContextMenu;
			ListViewItem selection = GetSelection();
			if (selection != null)
			{
				contextMenu.MenuItems.Clear();
				Wts.ConnectstateClass connectstateClass;
				switch (selection.SubItems[1].Text)
				{
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
				MenuItem menuItem = new MenuItem("断开连接", DisconnectSession);
				menuItem.Enabled = (connectstateClass == Wts.ConnectstateClass.Connected);
				MenuItem item = menuItem;
				contextMenu.MenuItems.Add(item);
				contextMenu.MenuItems.Add("-");
				item = new MenuItem("注销", LogOffSession);
				contextMenu.MenuItems.Add(item);
				item.Enabled = !Policies.DisableLogOff;
			}
		}

		private void DisconnectSession(object sender, EventArgs e)
		{
			ListViewItem selection = GetSelection();
			if (int.TryParse(selection.SubItems[0].Text, out int result))
			{
				_remoteSessions.DisconnectSession(result);
				RefreshSessions();
			}
		}

		private void LogOffSession(object sender, EventArgs e)
		{
			ListViewItem selection = GetSelection();
			if (int.TryParse(selection.SubItems[0].Text, out int result))
			{
				_remoteSessions.LogOffSession(result);
				RefreshSessions();
			}
		}

		private void ListSessionsForm_Resize(object sender, EventArgs e)
		{
			SessionListView.Width = RefreshButton.Right - SessionListView.Left;
		}

		private void SessionListView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			_sortOrder = new int[1];
			int num = 0;
			_sortOrder[num++] = e.Column;
			SortListView();
		}

		private void SortListView()
		{
			ArrayList arrayList = new ArrayList(SessionListView.Items.Count);
			foreach (ListViewItem item in SessionListView.Items)
			{
				arrayList.Add(item);
			}
			arrayList.Sort(new SessionListSortComparer(_sortOrder));
			SessionListView.Items.Clear();
			foreach (ListViewItem item2 in arrayList)
			{
				SessionListView.Items.Add(item2);
			}
		}

		private void ListSessionsForm_SizeChanged(object sender, EventArgs e)
		{
			int width = Math.Max(20, base.ClientRectangle.Width - SessionListView.Location.X * 2);
			int height = Math.Max(20, base.ClientRectangle.Height - SessionListView.Location.Y - 10);
			SessionListView.Size = new Size(width, height);
		}
	}
}
