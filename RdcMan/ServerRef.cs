using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan {
	public abstract class ServerRef : ServerBase {
		private readonly Server _server;

		public override Server ServerNode => _server;

		public override FileGroup FileGroup => _server.FileGroup;

		public override bool IsClientDocked => _server.IsClientDocked;

		public override bool IsClientUndocked => _server.IsClientUndocked;

		public override bool IsClientFullScreen => _server.IsClientFullScreen;

		public override bool IsConnected => _server.IsConnected;

		public override ServerSettings Properties => _server.Properties;

		public override CommonDisplaySettings DisplaySettings => _server.DisplaySettings;

		public override string RemoveTypeDescription => "服务器引用";

		public override DisplayStates DisplayState {
			get {
				return _server.DisplayState;
			}
			set {
				_server.DisplayState = value;
			}
		}

		public override Size Size {
			get {
				return _server.Size;
			}
			set {
				_server.Size = value;
			}
		}

		public override Point Location {
			get {
				return _server.Location;
			}
			set {
				_server.Location = value;
			}
		}

		protected ServerRef(Server server) {
			_server = server;
			_server.AddServerRef(this);
			base.Text = server.Text;
			ChangeImageIndex(ImageConstants.DisconnectedServer);
		}

		protected override void InitSettings() { }

		public override string ToString() {
			return $"{GetType().Name}: {base.Text}";
		}

		internal override void Show() {
			_server.Show();
		}

		internal override void Hide() {
			_server.Hide();
		}

		public override void Connect() {
			_server.Connect();
		}

		public override void ConnectAs(LogonCredentials logonSettings, ConnectionSettings connectionsettings) {
			_server.ConnectAs(logonSettings, connectionsettings);
		}

		public override void Reconnect() {
			_server.Reconnect();
		}

		public override void Disconnect() {
			_server.Disconnect();
		}

		public override void LogOff() {
			_server.LogOff();
		}

		public override void DoPropertiesDialog(Form parentForm, string activeTabName) {
			_server.DoPropertiesDialog(parentForm, activeTabName);
		}

		public override bool CanRemove(bool popUI) {
			return false;
		}

		public override bool CanDropOnTarget(RdcTreeNode targetNode) {
			GroupBase groupBase = (targetNode as GroupBase) ?? (targetNode.Parent as GroupBase);
			switch (groupBase.DropBehavior()) {
				case DragDropEffects.Copy:
					return groupBase.CanDropServers();
				case DragDropEffects.Link:
					if (groupBase.CanDropServers())
						return AllowEdit(popUI: false);
					return false;
				default:
					return false;
			}
		}

		public override void CollectNodesToInvalidate(bool recurseChildren, HashSet<RdcTreeNode> set) {
			set.Add(this);
			set.Add(base.Parent as RdcTreeNode);
		}

		internal override void WriteXml(XmlTextWriter tw) {
			throw new NotImplementedException();
		}

		public sealed override void ChangeImageIndex(ImageConstants index) {
			base.ImageIndex = _server.ImageIndex;
			base.SelectedImageIndex = _server.SelectedImageIndex;
		}

		internal override void Focus() {
			_server.Focus();
		}

		internal override void FocusConnectedClient() {
			_server.FocusConnectedClient();
		}

		internal virtual void OnRemoveServer() {
			ServerTree.Instance.RemoveNode(this);
		}

		public override void OnRemoving() {
			_server.RemoveServerRef(this);
		}

		internal override void GoFullScreen() {
			_server.GoFullScreen();
		}

		internal override void LeaveFullScreen() {
			_server.LeaveFullScreen();
		}

		internal override void Undock() {
			_server.Undock();
		}

		internal override void Dock() {
			_server.Dock();
		}

		internal override void ScreenCapture() {
			_server.ScreenCapture();
		}
	}
}
